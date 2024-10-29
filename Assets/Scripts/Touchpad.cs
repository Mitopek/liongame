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

    // Update is called once per frame
    void Update()
    {
        CheckTouch();
        
    }

    //check finger move
    void CheckTouch()
    {
        // Sprawdzanie dotyku na urządzeniach mobilnych
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

        // Sprawdzanie kliknięcia myszką na komputerze
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
        // Oblicz różnicę w pozycjach
        float x = endTouchPosition.x - startTouchPosition.x;
        float y = endTouchPosition.y - startTouchPosition.y;

        // Sprawdź, czy ruch był w poziomie, czy w pionie
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

        // Resetuj pozycje po przetworzeniu dotyku/kliknięcia
        startTouchPosition = Vector2.zero;
        endTouchPosition = Vector2.zero;
    }
}