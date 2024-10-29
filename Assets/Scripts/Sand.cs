using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : MonoBehaviour
{
    public GameObject footprint;
    bool isDone = false;
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
        if (other.tag == "PlayerCenterTrigger")
        {
            if (!isDone)
            {
                isDone = true;
                MapSystem.Instance.AddDoneSand();
                Transform parent = other.transform.parent;
                Instantiate(footprint, transform.position, parent.rotation);
            }
        }
    }
}
