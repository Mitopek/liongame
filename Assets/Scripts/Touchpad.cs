using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touchpad : MonoBehaviour
{
    // Start is called before the first frame update
    Vector3 startTouchPosition;
    Vector3 endTouchPosition;

    public GameObject player; 
    CharacterMovement playerScript;

    void Awake()
    {
        playerScript = player.GetComponent<CharacterMovement>();
    }

    void Start() 
    {
        
    }

    void Update()
    {
        CheckTouch();
        
    }

    void CheckTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                ProcessTouch();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            endTouchPosition = Input.mousePosition;
            ProcessTouch();
        }
    }

    void ProcessTouch()
    {
        float x = endTouchPosition.x - startTouchPosition.x;
        float y = endTouchPosition.y - startTouchPosition.y;

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            if (x > 0)
            {
                playerScript.nextMoveDirection = MoveDirectionType.Right;
            }
            else
            {
                playerScript.nextMoveDirection = MoveDirectionType.Left;
            }
        }
        else if(Mathf.Abs(x) < Mathf.Abs(y))
        {
            if (y > 0)
            {
                playerScript.nextMoveDirection = MoveDirectionType.Up;
            }
            else
            {
                playerScript.nextMoveDirection = MoveDirectionType.Down;
            }
        } else {
            if(playerScript.isMoving) {
                playerScript.nextMoveDirection = MoveDirectionType.None;
                return;
            }
            playerScript.nextMoveDirection = playerScript.lastMoveDirection;
        }
        startTouchPosition = Vector2.zero;
        endTouchPosition = Vector2.zero;
    }
}