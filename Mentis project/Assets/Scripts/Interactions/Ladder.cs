using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public 
    // Start is called before the first frame update
    void Start()
    {
        //GameObject.GetComponent<scr>;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("heehoo");
    }
}
