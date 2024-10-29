using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrows : MonoBehaviour
{
    public GameObject player;
    public GameObject arrowSprite;
    //moving time 3 seconds
    public float speed = 5f; // Dodano prędkość
    public float movingTime = 0.5f;
    float currentTime = 0f;

    float yOffset = -2f;
    MoveDirectionType nextMoveDirection;
    MoveDirectionType lastMoveDirection;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        nextMoveDirection = player.GetComponent<CharacterMovement>().nextMoveDirection;
        if (nextMoveDirection != lastMoveDirection)
        {
            lastMoveDirection = nextMoveDirection;
            currentTime = 0f;
        }
        if(nextMoveDirection == MoveDirectionType.None)
        {
            arrowSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            return;
        }
        RotateArrow(nextMoveDirection);
        MovingArrow();
    }

    void RotateArrow(MoveDirectionType direction)
    {
        switch(direction) {
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
                transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
        }
    }

    Vector2 getDirectionArrow(MoveDirectionType direction)
    {
        switch(direction) {
            case MoveDirectionType.Up:
                return new Vector2(0, 0.5f);
            case MoveDirectionType.Down:
                return new Vector2(0, -0.5f);
            case MoveDirectionType.Left:
                return new Vector2(-0.5f, 0);
            case MoveDirectionType.Right:
                return new Vector2(0.5f, 0);
            default:
                return Vector2.zero;
        }
    }

// moving and changing opacity and infinity in time this
    void MovingArrow()
    {
        currentTime += Time.deltaTime * speed; // Zwiększenie currentTime na podstawie speed
        if (currentTime >= movingTime)
        {
            currentTime = 0f;
        }

        float alpha = 0.75f - Mathf.PingPong(currentTime / movingTime, 1);
        arrowSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        
        // Ruch strzałki do kierunku i powrót na koniec czasu
        Vector2 direction = getDirectionArrow(nextMoveDirection);
        direction.y += yOffset; // Dodanie offsetu w dół na osi Y
        arrowSprite.transform.position = Vector2.Lerp(new Vector2(0, yOffset), direction, currentTime / movingTime);
    }
}
