using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationHelper : MonoBehaviour
{
    public Transform player;
    public LayerMask groundMask;
    public float rayLength = 5f;

    private void FixedUpdate()
    {
        bool hit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, rayLength, groundMask);
        if (hit)
        {
            transform.up = info.normal;
        }

        transform.Rotate(Vector3.up, player.transform.eulerAngles.y - transform.eulerAngles.y, Space.Self);

        Debug.DrawLine(transform.position, transform.position + transform.forward * 5f, Color.red);
    }
}