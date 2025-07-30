using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using Unity.VisualScripting;

[UpdateInGroup(typeof(InitializationSystemGroup))] 
public partial struct CameraInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitializeCameraTargetTag>();
    }
    public void OnUpdate(ref SystemState state)
    {
        if (CameraTargetSingleton.Instance == null) return;
        var cameraTargetTransform = CameraTargetSingleton.Instance.transform;

        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var ( cameraTarget, entity) 
            in SystemAPI.Query<RefRW<CameraTarget>>().WithAll<InitializeCameraTargetTag, PlayerTag, GhostOwnerIsLocal>().WithEntityAccess())
        {
            cameraTarget.ValueRW.CameraTransform = cameraTargetTransform;
            ecb.RemoveComponent<InitializeCameraTargetTag>(entity);
        }

        ecb.Playback(state.EntityManager);
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
            cameraTarget.CameraTransform.Value.position = transform.Position;
            
            // Also set up the camera's FollowTarget component if it exists
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