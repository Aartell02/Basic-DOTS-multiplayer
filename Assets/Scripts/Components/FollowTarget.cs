
using System;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [NonSerialized]
    public Transform target;     
    public Vector3 offset = new Vector3(0, 5, -10);
    //After Movement
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
