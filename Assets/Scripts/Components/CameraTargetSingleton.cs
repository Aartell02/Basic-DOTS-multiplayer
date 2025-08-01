using UnityEngine;

public class CameraTargetSingleton : MonoBehaviour
{
    public static CameraTargetSingleton Instance;
    public void Awake()
    {
        if(Instance != null)
        {
            //Debug.LogWarning("Multiple instances of cameratargetsingleton detected. Destroying new instance", Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}