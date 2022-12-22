using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ScrollObjects : MonoBehaviour
{
    public GameObject[] objectsToScroll = new GameObject[10];
    public float speed = 10;
    public bool lockX;
    public bool lockY;
    public bool lockZ;
    public bool negative;
    public Transform targetPoint;
    private float pointDistance;
    public float despawnRange;
    private Vector3 nextPosition;

    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject NotTree in objectsToScroll)
        {
            //if()
            nextPosition = NotTree.transform.position;

            pointDistance = Vector3.Distance(nextPosition, targetPoint.position);

            Vector3 newCalculatedPosition = Vector3.MoveTowards(nextPosition, targetPoint.position, speed * Time.deltaTime);

            if (pointDistance <= despawnRange) {newCalculatedPosition = this.transform.position;}

            if (!lockX) { nextPosition.x = newCalculatedPosition.x; }
            if (!lockY) { nextPosition.y = newCalculatedPosition.y; }
            if (!lockZ) { nextPosition.z = newCalculatedPosition.z; }

            NotTree.transform.position = nextPosition;

        }

    }

}
