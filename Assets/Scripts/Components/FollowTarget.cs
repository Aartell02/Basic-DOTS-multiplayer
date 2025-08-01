
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;     
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            transform.position = desiredPosition;
        }
    }
}
