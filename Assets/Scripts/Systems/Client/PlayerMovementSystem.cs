#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Debug.Log($"start{state.DebugName}");
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        foreach (var (moveSpeed, inputVector, velocity, localTransform, entity) in
             SystemAPI.Query<RefRO<PlayerMoveSpeed>, RefRO<PlayerInputVector>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>>()
            .WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        { 
            var input = inputVector.ValueRO.inputVector;
            
            var moveVector = input * moveSpeed.ValueRO.moveSpeed * deltaTime;
            velocity.ValueRW.Linear = new float3(moveVector,0f) ;

            // Player facing logic: flip Y rotation based on X movement
            if (math.abs(input.x) > 0.01f)
            {
                //Facing right (default): y = 0, facing left: y = 180
                float yRotation = input.x > 0 ? 90f : -90f;
                localTransform.ValueRW.Rotation = quaternion.Euler(0, math.radians(yRotation), 0);
            }
            else
            {
                float yRotation = 180;
                localTransform.ValueRW.Rotation = quaternion.Euler(0, math.radians(yRotation), 0);
            }
        }
    } 
}

[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]

public partial struct PlayerAnimatorSyncSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // animator reference
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (playerGameObjectPrefab, entity) in SystemAPI.Query<PlayerGOPrefab>().WithNone<PlayerAnimatorReference>().WithEntityAccess())
        {
            var newCompanionGameObject = Object.Instantiate(playerGameObjectPrefab.Prefab);
            var newAnimatorReference = new PlayerAnimatorReference
            {
                Animator = newCompanionGameObject.GetComponent<Animator>()
            };
            ecb.AddComponent(entity, newAnimatorReference);
        }
        // animation
        foreach (var (animatorReference, moveSpeed) in
            SystemAPI.Query<PlayerAnimatorReference, PlayerMoveSpeed>())
        {
            animatorReference.Animator.SetFloat("Speed", moveSpeed.moveSpeed);
        }
        // 
        foreach (var (animatorReference, entity) in
            SystemAPI.Query<PlayerAnimatorReference>().WithNone<PlayerGOPrefab, LocalTransform>().WithEntityAccess())
        {
            Object.Destroy(animatorReference.Animator.gameObject);
            ecb.RemoveComponent<PlayerAnimatorReference>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
#endif