using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mud : MonoBehaviour
{
    // Start is called before the first frame update
    bool isActivated = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //on trigger enter
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "PlayerCenterTrigger" && !isActivated)
        {
            PlayerState player = other.transform.parent.GetComponent<PlayerState>();
            player.SetInvisible(true);
            GetComponent<AudioSource>().Play();
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            isActivated = true;
        }
    }
}
