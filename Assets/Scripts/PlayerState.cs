using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject invisibleMaskPrefab;
    public GameObject funnyMaskPrefab;
    public GameObject poisonMaskPrefab;
    public GameObject stinkPrefab;
    float invisibleTime = 10.0f;
    float stinkTime = 10.0f;
    float funnyTime = 10.0f;

    public bool isFunny = false;
    public bool isInvisible = false;

    public bool isStinky = false;
    
    public bool isInBush = false;

    GameObject mask;
    GameObject stink;

    private Coroutine coroutineRef;
    CharacterMovement characterMovement;
    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if(mask != null) {
            mask.transform.rotation = transform.rotation;
        }
        if(mask != null && !characterMovement.isPushing)
        {
            mask.transform.position = transform.position;
        }
    }

    public IEnumerator MoveMaskOnPush(MoveDirectionType direction)
    {
        int totalSteps = 16; 
        float maxStepSize = -0.02f; 
        for (int i = 0; i < totalSteps; i++)
        {
            if(mask == null)
            {
                yield break;
            }
           
            float stepSize = maxStepSize * (1 - Mathf.Abs(i - totalSteps / 2) / (float)(totalSteps / 2));
            Vector3 directionVector = new Vector3(0, 0, 0);
            switch (direction)
            {
                case MoveDirectionType.Up:
                    directionVector = new Vector3(0, stepSize, 0);
                    break;
                case MoveDirectionType.Down:
                    directionVector = new Vector3(0, -stepSize, 0);
                    break;
                case MoveDirectionType.Left:
                    directionVector = new Vector3(-stepSize, 0, 0);
                    break;
                case MoveDirectionType.Right:
                    directionVector = new Vector3(stepSize, 0, 0);
                    break;
            }

            
            if (i < totalSteps / 2)
            {
                mask.transform.position += directionVector;
            }
            else
            {
                mask.transform.position -= directionVector;
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    public void SetInvisibleMask(bool value)
    {
        if(isInvisible)
        {
            if(coroutineRef != null)
            {
                StopCoroutine(coroutineRef);
            }
            if(mask != null)
            {
                Destroy(mask);
            }
        }
        isInvisible = value;
        mask = Instantiate(invisibleMaskPrefab, transform.position, Quaternion.identity);
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
        coroutineRef = StartCoroutine(RemoveInvisibleMask());
    }

    public void SetFunnyMask(bool value)
    {
        isFunny = value;
        mask = Instantiate(funnyMaskPrefab, transform.position, Quaternion.identity, transform);
        StartCoroutine(RemoveFunnyMask());
    }

    public void SetPoisonMask(bool value)
    {
        isStinky = value;
        stink = Instantiate(stinkPrefab, transform.position, Quaternion.identity, transform);
        mask = Instantiate(poisonMaskPrefab, transform.position, Quaternion.identity, transform);
        GetComponent<SpriteRenderer>().color = new Color(0.5f, 1, 0.5f, 1);
        StartCoroutine(RemovePoisonMask());
    }

    IEnumerator RemovePoisonMask()
    {
        Vector3 currentScale = stink.transform.localScale;
        yield return new WaitForSeconds(stinkTime - 4);
        for (int i = 0; i < 8; i++)
        {
            stink.transform.localScale = Vector3.zero;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            mask.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.25f);
            stink.transform.localScale = currentScale;
            GetComponent<SpriteRenderer>().color = new Color(0.5f, 1, 0.5f, 1);
            mask.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.25f);
        }
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        Destroy(stink);
        Destroy(mask);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.zero);
        isStinky = false;
    }

    IEnumerator RemoveInvisibleMask()
    {
        yield return new WaitForSeconds(invisibleTime - 4);
        for (int i = 0; i < 8; i++)
        {
            mask.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.25f);
            mask.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.25f);
        }
        isInvisible = false;
        coroutineRef = null;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.zero);
        Destroy(mask);
    }

    IEnumerator RemoveFunnyMask()
    {
        yield return new WaitForSeconds(funnyTime - 4);
        for (int i = 0; i < 8; i++)
        {
            mask.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.25f);
            mask.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.25f);
        }
        isFunny = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.zero);
        Destroy(mask);
    }
}
