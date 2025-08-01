using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

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
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var(transform,cameraTarget) 
            in SystemAPI.Query<LocalToWorld,CameraTarget>().WithAll<PlayerTag>().WithNone<InitializeCameraTargetTag>())
        {
            // Block till there's no NaN's
            float3 pos = transform.Position;
            if (math.any(math.isnan(pos)))
            {
                continue;
            }
            //Debug.Log($"camera{state.DebugName}");
            cameraTarget.CameraTransform.Value.position = transform.Position;
            //Set up the camera's FollowTarget component if it exists
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
    }
}