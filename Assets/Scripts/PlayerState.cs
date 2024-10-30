using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject maskPrefab;
    float invisibleTime = 10.0f;
    float maskTime = 10.0f;
    public bool isInvisible = false;
    public bool hasMask = false;
    
    public bool isInBush = false;

    GameObject mask;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(mask != null)
        {
            mask.transform.position = transform.position;
            mask.transform.rotation = transform.rotation;
        }
    }

    public void SetInvisible(bool value)
    {
        isInvisible = value;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
        StartCoroutine(RemoveInvisible());
    }

    public void SetMask(bool value)
    {
        hasMask = value;
        mask = Instantiate(maskPrefab, transform.position, Quaternion.identity);
        StartCoroutine(RemoveMask());
    }

    IEnumerator RemoveInvisible()
    {
        yield return new WaitForSeconds(invisibleTime - 4);
        for (int i = 0; i < 8; i++)
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.25f);
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.25f);
        }
        isInvisible = false;
    }

    IEnumerator RemoveMask()
    {
        yield return new WaitForSeconds(maskTime - 4);
        for (int i = 0; i < 8; i++)
        {
            mask.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.25f);
            mask.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.25f);
        }
        hasMask = false;
        Destroy(mask);
    }
}
