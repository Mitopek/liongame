using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : Movement{
    public bool canMask = false;
    public bool canColide = false;
    public bool canEat = false;
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
        MoveDirectionType? playerTargetDirection = GetPlayerTargetDirection();
        if(playerTargetDirection != null) {
            nextMoveDirection = playerTargetDirection.Value;
            return;
        }
        List<MoveDirectionType> usedDirections = new List<MoveDirectionType>();
        int maxTries = 20;
        while(true) {
            maxTries--;
            MoveDirectionType drawDirection = GetRandomDirection(usedDirections);
            Vector2Int? targetPositionIndexes = getTargetPositionIndexes(drawDirection);
            if(targetPositionIndexes == null) {
                usedDirections.Add(drawDirection);
            } else if(!CanMoveToPositionIndexes(targetPositionIndexes.Value)) {
                usedDirections.Add(drawDirection);
            } else {
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
        if(!isOnTarget) {
            if(CanMoveToPositionIndexes(currentPositionIndexes.Value)) {
                MoveDirectionType? oppositeDirection = GetOppositeDirection(lastMoveDirection);
                currentTargetPosition = MapSystem.Instance.getPositionFromXAndY(currentPositionIndexes.Value.x, currentPositionIndexes.Value.y);
                currentTargetPositionIndexes = currentPositionIndexes;
                currentMoveDirection = oppositeDirection.Value;
            } else {
                //TODO
            }
        }
        blockMovement = false;
    }

    MoveDirectionType? GetPlayerTargetDirection() {
        Vector2Int? playerPositionIndexes = CharacterMovement.Instance.currentPositionIndexes;
        PlayerState player = CharacterMovement.Instance.GetComponent<PlayerState>();
        if(!CanMoveToPositionIndexes(playerPositionIndexes.Value)) {
            return null;
        }
        if(player.isInvisible) {
            return null;
        }
        MoveDirectionType?[,] directionsMapArray = new MoveDirectionType?[maxRows, maxColumns];
        bool[,] visited = new bool[maxRows, maxColumns];
        directionsMapArray[playerPositionIndexes.Value.y, playerPositionIndexes.Value.x] = MoveDirectionType.None;
        int maxTries = 100;

        System.Random rand = new System.Random();
        bool reverseOuterLoop = rand.Next(2) == 1; 
        bool reverseInnerLoop = rand.Next(2) == 1; 
        while (true) {
            maxTries--;
            if (maxTries <= 0) {
                return null;
            }

            bool[,] visitedInLoop = new bool[maxRows, maxColumns];

            for (int y = (reverseOuterLoop ? maxRows - 1 : 0); 
                    reverseOuterLoop ? y >= 0 : y < maxRows; 
                    y += (reverseOuterLoop ? -1 : 1)) {

                for (int x = (reverseInnerLoop ? maxColumns - 1 : 0); 
                        reverseInnerLoop ? x >= 0 : x < maxColumns; 
                        x += (reverseInnerLoop ? -1 : 1)) {

                    if (directionsMapArray[y, x] != null && !visited[y, x] && !visitedInLoop[y, x]) {
                        int upY = y + 1;
                        int downY = y - 1;
                        int leftX = x - 1;
                        int rightX = x + 1;

                        if (upY < maxRows && directionsMapArray[upY, x] == null && CanMoveToPositionIndexes(new Vector2Int(x, upY))) {
                            directionsMapArray[upY, x] = MoveDirectionType.Up;
                            visitedInLoop[upY, x] = true;
                        }
                        if (downY >= 0 && directionsMapArray[downY, x] == null && CanMoveToPositionIndexes(new Vector2Int(x, downY))) {
                            directionsMapArray[downY, x] = MoveDirectionType.Down;
                            visitedInLoop[downY, x] = true;
                        }
                        if (leftX >= 0 && directionsMapArray[y, leftX] == null && CanMoveToPositionIndexes(new Vector2Int(leftX, y))) {
                            directionsMapArray[y, leftX] = MoveDirectionType.Left;
                            visitedInLoop[y, leftX] = true;
                        }
                        if (rightX < maxColumns && directionsMapArray[y, rightX] == null && CanMoveToPositionIndexes(new Vector2Int(rightX, y))) {
                            directionsMapArray[y, rightX] = MoveDirectionType.Right;
                            visitedInLoop[y, rightX] = true;
                        }
                        visited[y, x] = true;
                        visitedInLoop[y, x] = true;
                    }

                    if (directionsMapArray[y, x] != null && directionsMapArray[y, x] != MoveDirectionType.None && 
                        currentPositionIndexes.Value.x == x && currentPositionIndexes.Value.y == y) {

                        Debug.Log("FInd on " + maxTries);
                        MoveDirectionType? direction = GetDirectionFromTrace(new Vector2Int(x, y), directionsMapArray);

                        if (direction != null) {
                            Debug.Log("and direction XDDDDDD: " + direction.Value);
                            return direction.Value;
                        }
                        Debug.Log("and direction XDDDDDD: NULL " + 1);
                        return null;
                    }
                }
            }
        }
    }

    MoveDirectionType? GetDirectionFromTrace(Vector2Int positionIndexes, MoveDirectionType?[,] directionsMapArray) {
        HashSet<MoveDirectionType> usedDirections = new();
        List<MoveDirectionType> directions = new();
        Vector2Int? playerPositionIndexes = CharacterMovement.Instance.currentPositionIndexes;
        int maxTries = 100;
        while(true) {
            maxTries--;
            if(positionIndexes.x == playerPositionIndexes.Value.x && positionIndexes.y == playerPositionIndexes.Value.y) {
                if(directions.Count == 0) {
                    return null;
                }
                return directions[0];
            }
            MoveDirectionType? direction = directionsMapArray[positionIndexes.y, positionIndexes.x];
            Debug.Log("direction: " + direction + " " + positionIndexes.x + " " + positionIndexes.y);
            if(direction == null) {
                Debug.Log("on tries: " + maxTries);
                return null;
            }
            MoveDirectionType opositeDirection = GetOppositeDirection(direction.Value);
            if(opositeDirection == MoveDirectionType.None) {
                Debug.Log("opositeDirection: " + 1);
                return null;
            }
            if(usedDirections.Contains(direction.Value)) {
                Debug.Log("opositeDirection: " + 2);
                return null;
            }
            usedDirections.Add(opositeDirection);
            switch(opositeDirection) {
                case MoveDirectionType.Up:
                    positionIndexes.y++;
                    break;
                case MoveDirectionType.Down:
                    positionIndexes.y--;
                    break;
                case MoveDirectionType.Left:
                    positionIndexes.x--;
                    break;
                case MoveDirectionType.Right:
                    positionIndexes.x++;
                    break;
            }
            if(positionIndexes.x < 0 || positionIndexes.x >= maxColumns || positionIndexes.y < 0 || positionIndexes.y >= maxRows) {
                Debug.Log("opositeDirection: " + 3);
                return null;
            }
            Debug.Log("HAHAHAH: " + 5);
            directions.Add(opositeDirection);
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

    protected void OnMask() {
        Debug.Log("XD");
    }

    protected void OnColide() {
        blockMovement = true;
        Debug.Log("Michal");
        ResetMovement();
        StartCoroutine(ResetBlockMovement());
    }

    //on trigger enter
    protected void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player") {
            PlayerState player = other.transform.GetComponent<PlayerState>();
            if(player.hasMask && canMask) {
                OnMask();
            } else if(!player.isInvisible) {
                CharacterMovement.Instance.Die();
            }
        }
                //if enemy tag === monkey and other tag === player
                Debug.Log(gameObject.tag +"sd" + other.tag);
        if ((gameObject.tag == "Monkey" && other.tag == "Boar") || 
            (gameObject.tag == "Boar" && other.tag == "Monkey"))
        {
            Debug.Log("AAAAAAAAAAAAAAA");
            if(canColide) {
                OnColide();
                other.GetComponent<Enemy>().OnColide();
            }

        }
    }


}
