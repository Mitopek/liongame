using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class Movement : MonoBehaviour
{
    protected List<MapItemType>[,] mapItems; // Usunięcie inicjalizacji
    protected Tree[,] trees; // Usunięcie inicjalizacji
    protected float squareSize; // Usunięcie inicjalizacji
    public float speed;
    public bool allowFly;
    public bool allowBush;
    public bool stopOnTarget;
    public MoveDirectionType nextMoveDirection = MoveDirectionType.None;
    public MoveDirectionType currentMoveDirection = MoveDirectionType.None;
    public MoveDirectionType lastMoveDirection = MoveDirectionType.None;
    public Vector2Int? currentPositionIndexes = null;
    public Vector2? currentTargetPosition = null;
    public Vector2Int? currentTargetPositionIndexes = null;
    public bool isMoving = false;
    protected int maxColumns; // Usunięcie inicjalizacji
    protected int maxRows; // Usunięcie inicjalizacji
    protected bool isOnTarget = true;

    protected bool blockMovement = false;

    protected float blockMovementTime = 1.0f;

    protected abstract bool CanMoveToPositionIndexes(Vector2Int positionIndexes);

    protected virtual void Start()
    {
                // Inicjalizacja instancji
        mapItems = MapSystem.Instance.mapItems;
        trees = MapSystem.Instance.trees;
        squareSize = MapSystem.Instance.squareSize;
        maxColumns = MapSystem.Instance.columns;
        maxRows = MapSystem.Instance.rows;
        currentMoveDirection = nextMoveDirection;
        currentTargetPositionIndexes = new Vector2Int((int)MapSystem.Instance.getXFromPosition(transform.position.x), (int)MapSystem.Instance.getYFromPosition(transform.position.y));
        currentPositionIndexes = currentTargetPositionIndexes;
    }
    protected void UpdateTargetPosition(MoveDirectionType direction, Vector2Int targetPositionIndexes) {
        currentTargetPosition = getTargetPosition(direction);
        if(currentTargetPosition == null) {
            return;
        }
        currentTargetPositionIndexes = targetPositionIndexes;
        currentMoveDirection = direction;
        if(!isOnTarget) {
            MoveDirectionType equalizingDirection = GetEqualizingDirection(currentTargetPosition.Value);
            if(equalizingDirection != MoveDirectionType.None) {
                Vector2? equalizingTargetPosition = GetEqaualizingTargetPosition(currentTargetPosition.Value);
                if(equalizingTargetPosition != null) {
                    Vector2Int equalizingTargetPositionIndexes = MapSystem.Instance.getXAndYFromPosition(equalizingTargetPosition.Value);
                    if(currentTargetPositionIndexes != equalizingTargetPositionIndexes) {
                        currentTargetPosition = equalizingTargetPosition;
                        currentTargetPositionIndexes = equalizingTargetPositionIndexes;
                        currentMoveDirection = equalizingDirection;
                    }
                }
            }
        }
        RotateCharacter();
    }

    protected bool WantTurnAround() {
        if(nextMoveDirection == MoveDirectionType.None) {
            return false;
        }
        if(currentMoveDirection == MoveDirectionType.None) {
            return false;
        }
        if(currentMoveDirection == nextMoveDirection) {
            return false;
        }
        if(currentMoveDirection == MoveDirectionType.Up && nextMoveDirection == MoveDirectionType.Down) {
            return true;
        }
        if(currentMoveDirection == MoveDirectionType.Down && nextMoveDirection == MoveDirectionType.Up) {
            return true;
        }
        if(currentMoveDirection == MoveDirectionType.Left && nextMoveDirection == MoveDirectionType.Right) {
            return true;
        }
        if(currentMoveDirection == MoveDirectionType.Right && nextMoveDirection == MoveDirectionType.Left) {
            return true;
        }
        return false;
    }

    protected void ResetMovement() {
        if(isMoving) {
            lastMoveDirection = currentMoveDirection;
            isMoving = false;
        }
        nextMoveDirection = MoveDirectionType.None;
        currentMoveDirection = MoveDirectionType.None;
        currentTargetPosition = null;
    }

    protected MoveDirectionType GetEqualizingDirection(Vector2 targetPosition) {
        float currentX = getRoundedPosition(transform.position).x;
        float currentY = getRoundedPosition(transform.position).y;
        
        float targetX = getRoundedPosition(targetPosition).x;
        float targetY = getRoundedPosition(targetPosition).y;

        float epsilon = 0.01f; 

        if(currentX == targetX && Mathf.Abs(currentY - targetY) <= squareSize + epsilon) {
           return MoveDirectionType.None;     
        }
        if(currentY == targetY && Mathf.Abs(currentX - targetX) <= squareSize + epsilon) {
            return MoveDirectionType.None;
        }
        if(currentX > targetX && currentY == targetY) {
            return MoveDirectionType.Left;
        }
        if(currentX < targetX && currentY == targetY) {
            return MoveDirectionType.Right;
        }
        if(currentY > targetY && currentX == targetX) {
            return MoveDirectionType.Down;
        }
        if(currentY < targetY && currentX == targetX) {
            return MoveDirectionType.Up;
        }
        if(currentX != targetX && currentY != targetY) {
            if(Math.Abs(currentX - targetX) < Math.Abs(currentY - targetY)) {
                if(currentX > targetX) {
                    return MoveDirectionType.Left;
                } else {
                    return MoveDirectionType.Right;
                }
            } else {
                if(currentY > targetY) {
                    return MoveDirectionType.Down;
                } else {
                    return MoveDirectionType.Up;
                }
            }
        }
        return MoveDirectionType.None;
    }

    //jesli np. chcemy zrobic ruch w dol ale postac zostala zatrzymana zanim dotarla do celu to dajmy equalizingTargetPosition na pozycje która wyrownuje postac do siatki i ta pozycja jest po drodze do celu
    protected Vector2? GetEqaualizingTargetPosition(Vector2 targetPosition) {
        float currentX = getRoundedPosition(transform.position).x;
        float currentY = getRoundedPosition(transform.position).y;
        
        float targetX = getRoundedPosition(targetPosition).x;
        float targetY = getRoundedPosition(targetPosition).y;

        float epsilon = 0.01f; 

        if(currentX == targetX && Mathf.Abs(currentY - targetY) <= squareSize + epsilon) {
           return null;     
        }
        if(currentY == targetY && Mathf.Abs(currentX - targetX) <= squareSize + epsilon) {
            return null;
        }
        if(currentX > targetX && currentY == targetY) {
            return new Vector2(targetX + squareSize, targetY);
        }
        if(currentX < targetX && currentY == targetY) {
            return new Vector2(targetX - squareSize, targetY);
        }
        if(currentY > targetY && currentX == targetX) {
            return new Vector2(targetX, targetY + squareSize);
        }
        if(currentY < targetY && currentX == targetX) {
            return new Vector2(targetX, targetY - squareSize);
        }
        if(currentX != targetX && currentY != targetY) {
            if(Math.Abs(currentX - targetX) < Math.Abs(currentY - targetY)) {
                return new Vector2(targetX, currentY);
            } else {
                return new Vector2(currentX, targetY);
            }
        }
        return null;
    }


    protected Vector2? getTargetPosition(MoveDirectionType direction) {
        float currentX = MapSystem.Instance.getXFromPosition(transform.position.x);
        float currentY = MapSystem.Instance.getYFromPosition(transform.position.y);
        switch(direction) {
            case MoveDirectionType.Up:
                return MapSystem.Instance.getPositionFromXAndY((int)currentX, (int)currentY + 1);
            case MoveDirectionType.Down:
                return MapSystem.Instance.getPositionFromXAndY((int)currentX, (int)currentY - 1);
            case MoveDirectionType.Left:
                return MapSystem.Instance.getPositionFromXAndY((int)currentX - 1, (int)currentY);
            case MoveDirectionType.Right:
                return MapSystem.Instance.getPositionFromXAndY((int)currentX + 1, (int)currentY);
            default:
                return null;
        }
    }

    protected Vector2Int? getTargetPositionIndexes(MoveDirectionType direction) {
        float currentX = MapSystem.Instance.getXFromPosition(transform.position.x);
        float currentY = MapSystem.Instance.getYFromPosition(transform.position.y);


        switch(direction) {
            case MoveDirectionType.Up:
                return new Vector2Int((int)currentX, (int)currentY + 1);
            case MoveDirectionType.Down:
                return new Vector2Int((int)currentX, (int)currentY - 1);
            case MoveDirectionType.Left:
                return new Vector2Int((int)currentX - 1, (int)currentY);
            case MoveDirectionType.Right:
                return new Vector2Int((int)currentX + 1, (int)currentY);
            default:
                return null;
        }
    }

    protected Vector2 getRoundedPosition(Vector2 position) {
        return new Vector2(
            Mathf.Round(position.x * 100f) / 100f,
            Mathf.Round(position.y * 100f) / 100f
        );
    }
        

//remember about equalizingTargetPosition
    protected void RotateCharacter() {
        switch(currentMoveDirection) {
            case MoveDirectionType.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case MoveDirectionType.Down:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case MoveDirectionType.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MoveDirectionType.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }
}
