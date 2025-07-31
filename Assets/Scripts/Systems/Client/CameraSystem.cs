using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using Unity.VisualScripting;

[UpdateInGroup(typeof(InitializationSystemGroup))] 
public partial struct CameraInitializationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (CameraTargetSingleton.Instance == null) return;
        var cameraTargetTransform = CameraTargetSingleton.Instance.transform;
        foreach (var (cameraTarget, shouldInitialize, entity) 
            in SystemAPI.Query<RefRW<CameraTarget>, EnabledRefRW<InitializeCameraTargetTag>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        {
            cameraTarget.ValueRW.CameraTransform = cameraTargetTransform;
            shouldInitialize.ValueRW = false;
        }
    }
}
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct MoveCameraSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach(var(transform,cameraTarget) 
            in SystemAPI.Query<LocalToWorld,CameraTarget>().WithAll<PlayerTag>().WithNone<InitializeCameraTargetTag>())
        {
            Debug.Log($"camera{state.DebugName}");
            cameraTarget.CameraTransform.Value.position = transform.Position;
            
            //Also set up the camera's FollowTarget component if it exists

            var camera = Camera.main;
            if (camera != null)
            {
                var followTarget = camera.GetComponent<FollowTarget>();
                if (followTarget != null)
                {
                    followTarget.target = cameraTarget.CameraTransform.Value;
                }
            }
            
        }
        foreach (var (playerPrefab, entity) 
            in SystemAPI.Query<PlayerGOPrefab>().WithEntityAccess())
        {
            Debug.Log($"gameobjcet{state.DebugName}");
            var go = playerPrefab;
            if (go != null)
            {
                var transform = go.Prefab.transform;
                          // Dodatkowo ustaw kamerê g³ówn¹
                var camera = Camera.main;
                if (camera != null)
                {
                    transform = camera.transform;
                }
            }
        }
    }
}