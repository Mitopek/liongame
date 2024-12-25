using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : Movement{
    public GameObject eggPrefab;
    public GameObject sinkPrefab;
    public bool canEat = false;
    public bool canStink = false;
    public bool canFunny = false;
    public bool canFollowPlayer = true;
    public bool canCatch = true;
    public bool canLayEgg = false;
    bool isStinky = false;
    bool isFunny = false;
    bool isWatchingPlayer = false;
    bool isFlying = false;
    int eggChance = 5;

    float stinkTime = 10.0f;
    float funnyTime = 10.0f;
    float currentSpeed;

    protected override void Start()
    {
        base.Start();
        currentSpeed = speed;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isWatchingPlayer) {
            RotateToTarget(CharacterMovement.Instance.gameObject);
        }
        if(blockMovement) {
            return;
        }
        if(gameObject.tag == "Snake") {
            StartCoroutine(BlockOnPlayerNear());
        }
        if(nextMoveDirection == MoveDirectionType.None) {
            // ResetMovement();
            CreateNextMoveDirection();
            return;
        }
        //jesli doszedlismy do celu
        if(allowFly) {
            CheckFly();
        }
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
            MakeMovingAnimation();
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
            if(canLayEgg) {
                DrawLayEgg();
            }
        } else {
            isOnTarget = false;
            transform.position = Vector2.MoveTowards(transform.position, currentTargetPosition.Value, currentSpeed * Time.deltaTime);
            currentPositionIndexes = currentTargetPositionIndexes;
        }
    }

    void CreateNextMoveDirection() {
        if(canFollowPlayer && !isStinky) {
            MoveDirectionType? playerTargetDirection = GetPlayerTargetDirection();
            if(playerTargetDirection != null) {
                nextMoveDirection = playerTargetDirection.Value;
                return;
            }
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

    void CheckFly() {
        if(!isFlying && currentTargetPosition != null) {
            Vector2Int? targetPositionIndexes = MapSystem.Instance.getXAndYFromPosition(currentTargetPosition.Value);
            if(targetPositionIndexes != null) {
                if(HasObjectToFly(targetPositionIndexes.Value)) {
                    isFlying = true;
                    Animator animator = GetComponent<Animator>();
                    animator.SetBool("isFlying", true);
                    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                    spriteRenderer.sortingLayerName = "FlyingEnemy";
                    currentSpeed = speed / 2;
                }
            }
        } else if(isFlying && currentPositionIndexes != null && isOnTarget) {
            if(!HasObjectToFly(currentPositionIndexes.Value)) {
                isFlying = false;
                Animator animator = GetComponent<Animator>();
                animator.SetBool("isFlying", false);
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                spriteRenderer.sortingLayerName = "Enemy";
                currentSpeed = speed;
            }
        }
    }

    void DrawLayEgg() {
        if(mapItems[currentPositionIndexes.Value.y, currentPositionIndexes.Value.x].Count > 0) {
            foreach(MapItemType mapItem in mapItems[currentPositionIndexes.Value.y, currentPositionIndexes.Value.x]) {
                if(mapItem == MapItemType.Wall) {
                    return;
                }
                if(mapItem == MapItemType.Tree) {
                    return;
                }
                if(mapItem == MapItemType.Bush) {
                    return;
                }
            }
        }
        int random = UnityEngine.Random.Range(0, 100);
        if(random < eggChance) {
            Instantiate(eggPrefab, transform.position, Quaternion.identity);
        }
    }

    public void SetPee(float time) {
        if(!canEat) {
            return;
        }
        blockMovement = true;
        canCatch = false;
        Animator animator = GetComponent<Animator>();
        animator.SetBool("isPeeing", true);
        StartCoroutine(DesactivatePee(time));
    }

    IEnumerator DesactivatePee(float time) {
        yield return new WaitForSeconds(time);
        blockMovement = false;
        canCatch = true;
        Animator animator = GetComponent<Animator>();
        animator.SetBool("isPeeing", false);
    }

    IEnumerator BlockOnPlayerNear() {
        blockMovement = true;
        isWatchingPlayer = true;
        DisableMovingAnimation();
        //player state
        PlayerState player = CharacterMovement.Instance.GetComponent<PlayerState>();
        if(player.isInvisible || player.isInBush) {
            blockMovement = false;
            isWatchingPlayer = false;
            RotateCharacter();
            MakeMovingAnimation();
            yield break;
        }
        Transform playerTransform = CharacterMovement.Instance.transform;
        Vector2Int? playerPositionIndexes = MapSystem.Instance.getXAndYFromPosition(playerTransform.position);
        if(Math.Abs(playerPositionIndexes.Value.x - currentPositionIndexes.Value.x) > 2 || Math.Abs(playerPositionIndexes.Value.y - currentPositionIndexes.Value.y) > 2) {
            blockMovement = false;
            isWatchingPlayer = false;
            RotateCharacter();
            MakeMovingAnimation();
        } else {
            yield return new WaitForSeconds(1f);
            StartCoroutine(BlockOnPlayerNear());
        }
    }
    IEnumerator ResetBlockMovement() {
        yield return new WaitForSeconds(1f);
        blockMovement = false;
    }

//up down right or left - where is the nearest player
    void RotateToTarget(GameObject target) {
        float xDiff = target.transform.position.x - transform.position.x;
        float yDiff = target.transform.position.y - transform.position.y;
        if(Math.Abs(xDiff) > Math.Abs(yDiff)) {
            if(xDiff > 0) {
                transform.rotation = Quaternion.Euler(0, 0, 270);
            } else {
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        } else {
            if(yDiff > 0) {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            } else {
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }

    MoveDirectionType? GetPlayerTargetDirection() {
        //get player transform
        Transform playerTransform = CharacterMovement.Instance.transform;
        Vector2Int? playerPositionIndexes = MapSystem.Instance.getXAndYFromPosition(playerTransform.position);
        if(playerPositionIndexes == null) {
            return null;
        }
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
        //jesli pomiedzy graczem a botem jest jakas sciana lub drzewo to return 
        int yMin = Math.Min(playerPositionIndexes.Value.y, currentPositionIndexes.Value.y);
        int xMin = Math.Min(playerPositionIndexes.Value.x, currentPositionIndexes.Value.x);
        int yEnd = Math.Max(playerPositionIndexes.Value.y, currentPositionIndexes.Value.y);
        int xEnd = Math.Max(playerPositionIndexes.Value.x, currentPositionIndexes.Value.x);
        int maxRange = 3; //bird too :)
        if (yEnd - yMin > maxRange || xEnd - xMin > maxRange) {
            for(int i = yMin; i <= yEnd; i++) {
                for(int j = xMin; j <= xEnd; j++) {
                    if(mapItems[i, j].Count > 0) {
                        foreach(MapItemType mapItem in mapItems[i, j]) {
                            if(mapItem == MapItemType.Wall) {
                                return null;
                            }
                            if(mapItem == MapItemType.Tree) {
                                return null;
                            }
                            if(mapItem == MapItemType.Bush) {
                                return null;
                            }
                        }
                    }
                }
            }
        }
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
                        MoveDirectionType? direction = GetDirectionFromTrace(new Vector2Int(x, y), directionsMapArray);
                        Debug.Log(direction);
                        if (direction != null) {
                            return direction.Value;
                        }
                        return null;
                    }
                }
            }
        }
    }

    MoveDirectionType? GetDirectionFromTrace(Vector2Int positionIndexes, MoveDirectionType?[,] directionsMapArray) {
        HashSet<MoveDirectionType> usedDirections = new();
        List<MoveDirectionType> directions = new();
        Transform playerTransform = CharacterMovement.Instance.transform;
        Vector2Int? playerPositionIndexes = MapSystem.Instance.getXAndYFromPosition(playerTransform.position);
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
            if(direction == null) {
                return null;
            }
            MoveDirectionType opositeDirection = GetOppositeDirection(direction.Value);
            if(opositeDirection == MoveDirectionType.None) {
                return null;
            }
            if(usedDirections.Contains(direction.Value)) {
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
                return null;
            }
            directions.Add(opositeDirection);
        }
    }


    MoveDirectionType GetRandomDirection(List<MoveDirectionType> usedDirections) {
        MoveDirectionType upDirection = GetRotateDirection(MoveDirectionType.Up, currentMoveDirection);
        MoveDirectionType rightDirection = GetRotateDirection(MoveDirectionType.Right, currentMoveDirection);
        MoveDirectionType leftDirection = GetRotateDirection(MoveDirectionType.Left, currentMoveDirection);
        MoveDirectionType downDirection = GetRotateDirection(MoveDirectionType.Down, currentMoveDirection);
        Vector2Int? upPosition = getTargetPositionIndexes(upDirection);
        Vector2Int? rightPosition = getTargetPositionIndexes(rightDirection);
        Vector2Int? leftPosition = getTargetPositionIndexes(leftDirection);
        Vector2Int? downPosition = getTargetPositionIndexes(downDirection);
        int randomRange = 0;
        if(!usedDirections.Contains(upDirection)) {
            if(isStinky) {
                randomRange += 5;
            } else if(HasObjectToFly(upPosition.Value)) {
                randomRange += 5;
            } else {
                randomRange += 50;
            }
        }
        if(!usedDirections.Contains(rightDirection)) {
            if(isStinky) {
                randomRange += 5;
            } else if(HasObjectToFly(rightPosition.Value)) {
                randomRange += 5;
            } else {
                randomRange += 20;
            }
        }
        if(!usedDirections.Contains(leftDirection)) {
            if(isStinky) {
                randomRange += 5;
            } else if(HasObjectToFly(leftPosition.Value)) {
                randomRange += 5;
            } else {
                randomRange += 20;
            }
        }
        if(!usedDirections.Contains(downDirection)) {
            if(isStinky) {
                randomRange += 5;
            } else if(HasObjectToFly(downPosition.Value)) {
                randomRange += 5;
            } else {
                randomRange += 10;
            }
        }
        int random = UnityEngine.Random.Range(0, randomRange);
        int threshold = 0;

        if (!usedDirections.Contains(upDirection)) {
            if(isStinky) {
                threshold += 5;
            } else if(HasObjectToFly(upPosition.Value)) {
                threshold += 5;
            } else {
                threshold += 50;
            }
            if (random < threshold) {
                return upDirection;
            }
        }
        if (!usedDirections.Contains(rightDirection)) {
            if(isStinky) {
                threshold += 5;
            } else if(HasObjectToFly(rightPosition.Value)) {
                threshold += 5;
            } else {
                threshold += 20;
            }
            if (random < threshold) {
                return rightDirection;
            }
        }
        if (!usedDirections.Contains(leftDirection)) {
            if(isStinky) {
                threshold += 5;
            } else if(HasObjectToFly(leftPosition.Value)) {
                threshold += 5;
            } else {
                threshold += 20;
            }
            if (random < threshold) {
                return leftDirection;
            }
        }
        if (!usedDirections.Contains(downDirection)) {
            if(isStinky) {
                threshold += 5;
            } else if(HasObjectToFly(downPosition.Value)) {
                threshold += 5;
            } else {
                threshold += 10;
            }
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
                if(mapItem == MapItemType.Wall && (!allowFly || isStinky)) {
                    return false;
                }
                if(mapItem == MapItemType.Tree && (!allowFly || isStinky)) {
                    return false;
                }
                if(mapItem == MapItemType.Bush && !allowBush) {
                    return false;
                }
                if(mapItem == MapItemType.Hole) {
                    return false;
                }
            }
        }
        return true;
    }

    protected bool HasObjectToFly(Vector2Int positionIndexes) {
        if(positionIndexes.x < 0 || positionIndexes.x >= maxColumns || positionIndexes.y < 0 || positionIndexes.y >= maxRows) {
            return false;
        }
        if(mapItems[positionIndexes.y, positionIndexes.x].Count > 0) {
            foreach(MapItemType mapItem in mapItems[positionIndexes.y, positionIndexes.x]) {
                if(mapItem == MapItemType.Wall) {
                    return true;
                }
                if(mapItem == MapItemType.Tree) {
                    return true;
                }
            }
        }
        return false;
    }

    protected void OnStinky() {
        isStinky = true;
        canCatch = false;
        currentSpeed = speed / 3;
        Instantiate(sinkPrefab, transform.position, Quaternion.identity, transform);
        StartCoroutine(RemoveStink());
    }

    protected void OnFunny() {
        isFunny = true;
        canCatch = false;
        blockMovement = true;
        RotateToTarget(CharacterMovement.Instance.gameObject);
        Animator animator = GetComponent<Animator>();
        animator.SetBool("isFunny", true);
        StartCoroutine(RemoveFunny());
    }

    IEnumerator RemoveStink() {
        yield return new WaitForSeconds(stinkTime);
        isStinky = false;
        canCatch = true;
        currentSpeed = speed;
        Destroy(transform.GetChild(0).gameObject);
    }

    IEnumerator RemoveFunny() {
        yield return new WaitForSeconds(funnyTime);
        isFunny = false;
        canCatch = true;
        blockMovement = false;
        RotateCharacter();
        Animator animator = GetComponent<Animator>();
        animator.SetBool("isFunny", false);
    }

    //on trigger enter
    protected void OnTriggerStay2D(Collider2D other) {
        if(other.tag == "Player") {
            PlayerState player = other.transform.GetComponent<PlayerState>();
            if(player.isStinky && canStink && !isStinky && !isFunny) {
                OnStinky();
            } else if(player.isFunny && canFunny && !isStinky && !isFunny) {
                OnFunny();
            } else if(!player.isInvisible && canCatch) {
                CharacterMovement.Instance.Die();
            }
        }
    }
}
