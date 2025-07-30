using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;     
    public Vector3 offset = new Vector3(0, 5, -10);

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
