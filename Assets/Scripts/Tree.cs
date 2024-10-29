using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public TreeOrientationType orientation;
    private TreeOrientationType lastOrientation;
    private bool isRotating = false;

    public struct RotationResult
    {
        public Tree tree;
        public bool clockwise;

        public RotationResult(Tree tree, bool clockwise)
        {
            this.tree = tree;
            this.clockwise = clockwise;
        }
    }
    [SerializeField] private float rotationSpeed = 7.0f; 
    private float targetRotationZ;
    void Start()
    {
        orientation = GetOrientationByRotation();
        lastOrientation = orientation;
        StartCoroutine(test());
    }

    IEnumerator test()
    {
        yield return new WaitForSeconds(2);
        // Rotate(true);
    }

    void Update()
    {
        if (isRotating)
        {
            Rotating();
        }
    }

    public void Rotate(bool clockwise)
    {
        if(isRotating) {
            return;
        }
        isRotating = true;
        orientation = GetNextOrientation(); 
        SetTargetRotation(clockwise); 
    }

    private void SetTargetRotation(bool clockwise)
    {
        float currentRotationZ = transform.eulerAngles.z;
        if (clockwise)
        {
            targetRotationZ = currentRotationZ - 90f; 
        }
        else
        {
            targetRotationZ = currentRotationZ + 90f; 
        }
        targetRotationZ = NormalizeAngle(targetRotationZ);
    }

    private void Rotating()
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetRotationZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed * 3);
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
        {
            transform.rotation = targetRotation; 
            isRotating = false; 
            lastOrientation = orientation;
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle < 0) angle += 360;
        while (angle >= 360) angle -= 360;
        return angle;
    }

    private TreeOrientationType GetNextOrientation()
    {
        if (lastOrientation == TreeOrientationType.Horizontal)
        {
            return TreeOrientationType.Vertical;
        }
        return TreeOrientationType.Horizontal;
    }

    TreeOrientationType GetOrientationByRotation()
    {
        float zRotation = Mathf.Round(transform.eulerAngles.z);
        float normalizedRotation = NormalizeAngle(zRotation);
        if (normalizedRotation == 0 || normalizedRotation == 180)
        {
            return TreeOrientationType.Horizontal;
        }
        else if (normalizedRotation == 90 || normalizedRotation == 270)
        {
            return TreeOrientationType.Vertical;
        }
        return TreeOrientationType.Horizontal;
    }

    public bool? ClockwiseDirectionPlayerCanPush() {
        int treeX = MapSystem.Instance.getXFromPosition(transform.position.x);
        int treeY = MapSystem.Instance.getYFromPosition(transform.position.y);

        Vector2Int? playerPosition = CharacterMovement.Instance.currentPositionIndexes;
        Debug.Log(playerPosition.Value + "aaa");
        if(orientation == TreeOrientationType.Horizontal) {
            if(playerPosition == new Vector2Int(treeX - 1, treeY + 1) || playerPosition == new Vector2Int(treeX + 1, treeY - 1)) {
                return false;
            }
            if(playerPosition == new Vector2Int(treeX - 1, treeY - 1) || playerPosition == new Vector2Int(treeX + 1, treeY + 1)) {
                return true;
            }
        } else {
            if(playerPosition == new Vector2Int(treeX - 1, treeY - 1) || playerPosition == new Vector2Int(treeX + 1, treeY + 1)) {
                return false;
            }
            if(playerPosition == new Vector2Int(treeX - 1, treeY + 1) || playerPosition == new Vector2Int(treeX + 1, treeY - 1)) {
                return true;
            }
        }
        return null;
    }

    public (bool canRotate, RotationResult[] results) CanRotate(bool clockwise)
    {
        int treeX = MapSystem.Instance.getXFromPosition(transform.position.x);
        int treeY = MapSystem.Instance.getYFromPosition(transform.position.y);

        Tree[,] trees = MapSystem.Instance.trees;

        bool canRotate = true;
        List<RotationResult> results = new List<RotationResult>();
        results.Add(new RotationResult(this, clockwise));
        Vector2Int? playerPosition = CharacterMovement.Instance.currentPositionIndexes;

        if(clockwise) {
            if(orientation == TreeOrientationType.Horizontal) {
                int x = treeX - 1;
                int y = treeY + 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                Tree tree= trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Vertical) {
                        canRotate = false;
                    } else if(tree.orientation == TreeOrientationType.Horizontal) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX;
                y = treeY + 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Vertical) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX;
                y = treeY - 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Vertical) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX + 1;
                y = treeY - 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Horizontal) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    } else if(tree.orientation == TreeOrientationType.Vertical) {
                        canRotate = false;
                    }
                }
            } else {
                int x = treeX + 1;
                int y = treeY + 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                Tree tree= trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Horizontal) {
                        canRotate = false;
                    } else if(tree.orientation == TreeOrientationType.Vertical) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX + 1;
                y = treeY;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Horizontal) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX - 1;
                y = treeY;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Horizontal) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX - 1;
                y = treeY - 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Vertical) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    } else if(tree.orientation == TreeOrientationType.Horizontal) {
                        canRotate = false;
                    }
                }
            }
        } else {
            if(orientation == TreeOrientationType.Horizontal) {
                int x = treeX - 1;
                int y = treeY - 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                Tree tree= trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Vertical) {
                        canRotate = false;
                    } else if(tree.orientation == TreeOrientationType.Horizontal) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX;
                y = treeY - 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Vertical) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX;
                y = treeY + 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Vertical) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX + 1;
                y = treeY + 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Horizontal) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    } else if(tree.orientation == TreeOrientationType.Vertical) {
                        canRotate = false;
                    }
                }
            } else {
                int x = treeX + 1;
                int y = treeY - 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                Tree tree= trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Horizontal) {
                        canRotate = false;
                    } else if(tree.orientation == TreeOrientationType.Vertical) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX + 1;
                y = treeY;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Horizontal) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX - 1;
                y = treeY;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Horizontal) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    }
                }
                x = treeX - 1;
                y = treeY + 1;
                if(playerPosition == new Vector2Int(x, y)) {
                    canRotate = false;
                }
                tree = trees[y, x];
                if(tree != null) {
                    if(tree.orientation == TreeOrientationType.Vertical) {
                        var result = tree.CanRotate(!clockwise);
                        if(!result.canRotate) {
                            canRotate = false;
                        }
                        results.Add(new RotationResult(tree, !clockwise));
                        if(result.results.Length > 0) {
                            foreach(RotationResult rotationResult in result.results) {
                                results.Add(rotationResult);
                            }
                        }
                    } else if(tree.orientation == TreeOrientationType.Horizontal) {
                        canRotate = false;
                    }
                }
            }
        }
        return (canRotate, results.ToArray());
    }
}
