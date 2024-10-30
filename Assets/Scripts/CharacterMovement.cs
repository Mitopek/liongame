using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CharacterMovement : Movement
{
    public static CharacterMovement Instance { get; private set; }

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
            ResetMovement();
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
                targetPositionIndexes = getTargetPositionIndexes(currentMoveDirection);
                if(targetPositionIndexes == null) {
                    ResetMovement();
                    return;
                }
                if(!CanMoveToPositionIndexes(targetPositionIndexes.Value)) {
                    ResetMovement();
                    return;
                }
                UpdateTargetPosition(currentMoveDirection, targetPositionIndexes.Value);
                if(CheckTargetTree(currentTargetPositionIndexes.Value)) {
                    return;
                }
                isMoving = true;
                return;
            }
            UpdateTargetPosition(nextMoveDirection, targetPositionIndexes.Value);
            if(CheckTargetTree(currentTargetPositionIndexes.Value)) {
                return;
            }
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
            if(stopOnTarget || IsInBush(currentTargetPosition.Value)) {
                nextMoveDirection = MoveDirectionType.None;
            }
            currentTargetPosition = null;
        } else {
            isOnTarget = false;
            transform.position = Vector2.MoveTowards(transform.position, currentTargetPosition.Value, speed * Time.deltaTime);
            currentPositionIndexes = currentTargetPositionIndexes;
        }
    }

    bool CheckTargetTree(Vector2Int? targetPositionIndexes) {
        var (hasTree, canShake, canRotate) = HasTreesToRotate(targetPositionIndexes.Value);
        if(hasTree) {
            if(!canShake && !canRotate) {
                ResetMovement();
                return true;
            }
            OnTreePushed();
            if(!canRotate) {
                ResetMovement();
            }
            return true;
        }
        return false;
    }

    void OnTreePushed() {
        blockMovement = true;
        StartCoroutine(UnblockMovement());
    }

    IEnumerator UnblockMovement() {
        yield return new WaitForSeconds(blockMovementTime);
        blockMovement = false;
    }

    public void Die() {
        Debug.Log("Died");
    }


    bool IsInBush(Vector2 position) {
        int currentX = (int)MapSystem.Instance.getXFromPosition(position.x);
        int currentY = (int)MapSystem.Instance.getYFromPosition(position.y);

        if(mapItems[currentY, currentX].Count > 0) {
            foreach(MapItemType mapItem in mapItems[currentY, currentX]) {
                if(mapItem == MapItemType.Bush) {
                    return true;
                }
            }
        }
        return false;
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
                    Tree tree = trees[positionIndexes.y, positionIndexes.x];
                    bool? isClockwise = tree.ClockwiseDirectionPlayerCanPush();
                    if(isClockwise != null) {
                        return true;
                    }
                    return false;
                }
                if(mapItem == MapItemType.Bush && !allowBush) {
                    return false;
                }
            }
        }
        return true;
    }

    (bool hasTree, bool canShake, bool canRotate)HasTreesToRotate(Vector2Int positionIndexes) {
        if(positionIndexes.x < 0 || positionIndexes.x >= maxColumns || positionIndexes.y < 0 || positionIndexes.y >= maxRows) {
            return (false, false, false);
        }
        if(mapItems[positionIndexes.y, positionIndexes.x].Count > 0) {
            foreach(MapItemType mapItem in mapItems[positionIndexes.y, positionIndexes.x]) {
                if(mapItem == MapItemType.Tree) {
                    Tree tree = trees[positionIndexes.y, positionIndexes.x];
                    bool? isClockwise = tree.ClockwiseDirectionPlayerCanPush();
                    Debug.Log("XD");
                    if(isClockwise != null) {
                        Debug.Log("XD2");
                        var result = tree.CanRotate(isClockwise.Value);
                        Debug.Log(isClockwise);
                        if(result.canRotate) {
                            foreach(Tree.RotationResult rotationResult in result.results) {
                                tree = rotationResult.tree;
                                tree.Rotate(rotationResult.clockwise);
                            }
                        }
                        return (true, !result.canRotate, result.canRotate);
                    }
                    return (true, false, false);
                }
            }
        }
        return (false, false, false);
    }
}
