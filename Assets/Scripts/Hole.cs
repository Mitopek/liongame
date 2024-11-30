using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    // Start is called before the first frame update

    public Sprite holeOpenedSprite;
    public Sprite holeClosedSprite;
    bool isOpened = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenHole()
    {
        isOpened = true;
        GetComponent<SpriteRenderer>().sprite = holeOpenedSprite;
    }

    public void CloseHole()
    {
        isOpened = false;
        GetComponent<SpriteRenderer>().sprite = holeClosedSprite;
    }

    //on trigger enter
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (isOpened)
            {
                Debug.Log("Congratulations! You have won!");
            }
        }
    }
}
