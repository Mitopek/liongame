using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : Movement
{
    public static Enemy Instance { get; private set; }

    void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    protected override void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    void Update()
    {
        // jesli ruch wylaczony
        if(blockMovement) {
            return;
        }
        if(nextMoveDirection == MoveDirectionType.None) {
            // ResetMovement();
            CreateNextMoveDirection();
            return;
        }
        //jesli doszedlismy do celu
        if(!isMoving) {
            Vector2Int? targetPositionIndexes = getTargetPositionIndexes(nextMoveDirection);
            if(targetPositionIndexes == null) {
                ResetMovement();
                return;
            }
            if(!CanMoveToPositionIndexes(targetPositionIndexes.Value)) {
                ResetMovement();
                return;
            }
            UpdateTargetPosition(nextMoveDirection, targetPositionIndexes.Value);
            isMoving = true;
            // equalizingTargetPosition = GetEqaualizingTargetPosition(targetPosition);
        } else if(WantTurnAround()) {
            Vector2Int? targetPositionIndexes = getTargetPositionIndexes(nextMoveDirection);
            if(targetPositionIndexes == null) {
                ResetMovement();
                return;
            }
            UpdateTargetPosition(nextMoveDirection, targetPositionIndexes.Value);
        }
        if(currentTargetPosition == null) {
           return;
        }
        if(Vector2.Distance(transform.position, currentTargetPosition.Value) < 0.01f) {
            transform.position = currentTargetPosition.Value;
            isMoving = false;
            isOnTarget = true;
            nextMoveDirection = MoveDirectionType.None;
            currentTargetPosition = null;
        } else {
            isOnTarget = false;
            transform.position = Vector2.MoveTowards(transform.position, currentTargetPosition.Value, speed * Time.deltaTime);
            currentPositionIndexes = currentTargetPositionIndexes;
        }
    }

    void CreateNextMoveDirection() {
        Debug.Log(currentMoveDirection);
        List<MoveDirectionType> usedDirections = new List<MoveDirectionType>();
        int maxTries = 20;
        while(true) {
            maxTries--;
            MoveDirectionType drawDirection = GetRandomDirection(usedDirections);
            Debug.Log("drawDirection: " + drawDirection);
            Vector2Int? targetPositionIndexes = getTargetPositionIndexes(drawDirection);
            if(targetPositionIndexes == null) {
                usedDirections.Add(drawDirection);
            } else if(!CanMoveToPositionIndexes(targetPositionIndexes.Value)) {
                usedDirections.Add(drawDirection);
            } else {
                Debug.Log("drawDirection so: " + drawDirection);
                nextMoveDirection = drawDirection;
                break;
            }
            if(maxTries <= 0) {
                break;
            }
        }
        if(nextMoveDirection == MoveDirectionType.None) {
            blockMovement = true;
            StartCoroutine(ResetBlockMovement());
        }
    }

    IEnumerator ResetBlockMovement() {
        yield return new WaitForSeconds(1f);
        blockMovement = false;
    }

    
    MoveDirectionType GetPlayerTargetDirection() {
        Vector2Int? playerPositionIndexes = CharacterMovement.Instance.currentPositionIndexes;
        MoveDirectionType?[,] directionsMapArray = new MoveDirectionType?[maxRows, maxColumns];
        directionsMapArray[playerPositionIndexes.Value.y, playerPositionIndexes.Value.x] = MoveDirectionType.None;
        while(true) {
            for (int y = 0; y < maxRows; y++) {
                for (int x = 0; x < maxColumns; x++) {
                    if(directionsMapArray[y, x] == MoveDirectionType.None) {
                        int upY = y + 1;
                        int downY = y - 1;
                        int leftX = x - 1;
                        int rightX = x + 1;
                        if(upY < maxRows && directionsMapArray[upY, x] == null) {
                            directionsMapArray[upY, x] = MoveDirectionType.Up;
                        }
                        if(downY >= 0 && directionsMapArray[downY, x] == null) {
                            directionsMapArray[downY, x] = MoveDirectionType.Down;
                        }
                        if(leftX >= 0 && directionsMapArray[y, leftX] == null) {
                            directionsMapArray[y, leftX] = MoveDirectionType.Left;
                        }
                        if(rightX < maxColumns && directionsMapArray[y, rightX] == null) {
                            directionsMapArray[y, rightX] = MoveDirectionType.Right;
                        }
                    }
                    if(directionsMapArray[y, x] != null && directionsMapArray[y, x] != MoveDirectionType.None && currentPositionIndexes.Value.x == x && currentPositionIndexes.Value.y == y) {
                        CreateTraceFromPositionIndexes(new Vector2Int(x, y), directionsMapArray);
                    }
        }
    }


    MoveDirectionType GetRandomDirection(List<MoveDirectionType> usedDirections) {
        MoveDirectionType upDirection = GetRotateDirection(MoveDirectionType.Up, currentMoveDirection);
        MoveDirectionType rightDirection = GetRotateDirection(MoveDirectionType.Right, currentMoveDirection);
        MoveDirectionType leftDirection = GetRotateDirection(MoveDirectionType.Left, currentMoveDirection);
        MoveDirectionType downDirection = GetRotateDirection(MoveDirectionType.Down, currentMoveDirection);
        int randomRange = 0;
        if(!usedDirections.Contains(upDirection)) {
            randomRange += 50;
        }
        if(!usedDirections.Contains(rightDirection)) {
            randomRange += 20;
        }
        if(!usedDirections.Contains(leftDirection)) {
            randomRange += 20;
        }
        if(!usedDirections.Contains(downDirection)) {
            randomRange += 10;
        }
        int random = UnityEngine.Random.Range(0, randomRange);
        Debug.Log("random: " + random +" "+ randomRange);
        int threshold = 0;

        if (!usedDirections.Contains(upDirection)) {
            threshold += 50;
            if (random < threshold) {
                return upDirection;
            }
        }
        if (!usedDirections.Contains(rightDirection)) {
            threshold += 20;
            if (random < threshold) {
                return rightDirection;
            }
        }
        if (!usedDirections.Contains(leftDirection)) {
            threshold += 20;
            if (random < threshold) {
                return leftDirection;
            }
        }
        if (!usedDirections.Contains(downDirection)) {
            threshold += 10;
            if (random < threshold) {
                return downDirection;
            }
        }
        return MoveDirectionType.None;
    } 

    MoveDirectionType GetRotateDirection(MoveDirectionType direction, MoveDirectionType currentDirection) {
        if(direction == MoveDirectionType.Up) {
            if(currentDirection == MoveDirectionType.Right) {
                return MoveDirectionType.Right;
            } else if(currentDirection == MoveDirectionType.Left) {
                return MoveDirectionType.Left;
            } else if(currentDirection == MoveDirectionType.Down) {
                return MoveDirectionType.Down;
            } else if(currentDirection == MoveDirectionType.Up) {
                return MoveDirectionType.Up;
            }
        } else if(direction == MoveDirectionType.Right) {
            if(currentDirection == MoveDirectionType.Up) {
                return MoveDirectionType.Right;
            } else if(currentDirection == MoveDirectionType.Left) {
                return MoveDirectionType.Up;
            } else if(currentDirection == MoveDirectionType.Down) {
                return MoveDirectionType.Left;
            } else if(currentDirection == MoveDirectionType.Right) {
                return MoveDirectionType.Down;
            }
        } else if(direction == MoveDirectionType.Left) {
            if(currentDirection == MoveDirectionType.Up) {
                return MoveDirectionType.Left;
            } else if(currentDirection == MoveDirectionType.Right) {
                return MoveDirectionType.Up;
            } else if(currentDirection == MoveDirectionType.Down) {
                return MoveDirectionType.Right;
            } else if(currentDirection == MoveDirectionType.Left) {
                return MoveDirectionType.Down;
            }
        } else if(direction == MoveDirectionType.Down) {
            if(currentDirection == MoveDirectionType.Up) {
                return MoveDirectionType.Down;
            } else if(currentDirection == MoveDirectionType.Right) {
                return MoveDirectionType.Left;
            } else if(currentDirection == MoveDirectionType.Left) {
                return MoveDirectionType.Right;
            } else if(currentDirection == MoveDirectionType.Down) {
                return MoveDirectionType.Up;
            }
        }
        return direction;
    } 


    protected override bool CanMoveToPositionIndexes(Vector2Int positionIndexes) {
        if(positionIndexes.x < 0 || positionIndexes.x >= maxColumns || positionIndexes.y < 0 || positionIndexes.y >= maxRows) {
            return false;
        }
        if(mapItems[positionIndexes.y, positionIndexes.x].Count > 0) {
            foreach(MapItemType mapItem in mapItems[positionIndexes.y, positionIndexes.x]) {
                if(mapItem == MapItemType.Wall && !allowFly) {
                    return false;
                }
                if(mapItem == MapItemType.Tree && !allowFly) {
                    return false;
                }
                if(mapItem == MapItemType.Bush && !allowBush) {
                    return false;
                }
            }
        }
        return true;
    }

}
