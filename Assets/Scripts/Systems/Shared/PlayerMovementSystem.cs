#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Windows;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnStart(ref SystemState state)
    {
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<PhysicsVelocity>();
        state.RequireForUpdate<PlayerAnimatorReference>();
        state.RequireForUpdate<PlayerTransformReference>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        float deltaTime = SystemAPI.Time.DeltaTime;
        foreach (var (playerSpeed, inputVector, velocity, localTransform, entity) in
             SystemAPI.Query<RefRW<PlayerSpeed>, RefRO<PlayerInputVector>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>>()
            .WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        {
            var input = inputVector.ValueRO.inputVector;
            //Debug.Log($"input: {input}");

            bool isMoving = playerSpeed.ValueRW.currentSpeed > 0.05f;
            bool isTryingToMove = math.lengthsq(input) > 0f;

            if (isTryingToMove)
            {
                // Accelerate
                playerSpeed.ValueRW.currentSpeed += playerSpeed.ValueRW.acceleration * deltaTime;
                playerSpeed.ValueRW.currentSpeed = math.min(playerSpeed.ValueRW.currentSpeed, playerSpeed.ValueRW.maxSpeed);
            }
            else if (isMoving)
            {
                // Decelerate
                playerSpeed.ValueRW.currentSpeed -= (playerSpeed.ValueRW.acceleration) * deltaTime;
                playerSpeed.ValueRW.currentSpeed = math.max(playerSpeed.ValueRW.currentSpeed, 0f);
            }

            // Last direction
            float2 lastDir = velocity.ValueRW.Linear.xy;
            if (math.lengthsq(lastDir) < 0.0001f) lastDir = new float2(0, 1); 
            float2 moveDirection = isTryingToMove ? input : math.normalize(lastDir);


            var moveVector = moveDirection * playerSpeed.ValueRW.currentSpeed * deltaTime;
            velocity.ValueRW.Linear = new float3(moveVector.x, moveVector.y, velocity.ValueRW.Linear.z);


            // Player facing flip
            if (math.abs(input.x) > 0.05f)
            {
                float yRotation = input.x > 0 ? 90f : -90f;
                localTransform.ValueRW.Rotation = quaternion.Euler(0, math.radians(yRotation), 0);
            }
            else
            {
                if (playerSpeed.ValueRO.currentSpeed < 0.05) 
                {
                    float yRotation = 180;
                    localTransform.ValueRW.Rotation = quaternion.Euler(0, math.radians(yRotation), 0);
                }

            }
            //Debug.Log($"Entity:{entity} | Rotation: {localTransform.ValueRW.Rotation} | {playerSpeed.ValueRW.currentSpeed}");
        }
        //Manual GO offset
        foreach (var (viewRef, transform, entity) in SystemAPI.Query<PlayerTransformReference, RefRW<LocalTransform>>().WithEntityAccess())
        {    
            // Block till there's no NaN's
            float3 pos = transform.ValueRO.Position;
            if (math.any(math.isnan(pos)))
            {
                continue;
            }
            //Debug.Log($"Location sync: {entity}");
            viewRef.Transform.position = transform.ValueRW.Position + new float3(0, -1.035f, 0);
            viewRef.Transform.rotation = transform.ValueRW.Rotation ; 
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
        foreach (var (playerGameObjectPrefab, transform, entity) in SystemAPI.Query<PlayerGOPrefab, RefRW<LocalTransform>>().WithNone<PlayerAnimatorReference>().WithEntityAccess())
        {
            var newCompanionGameObject = Object.Instantiate(playerGameObjectPrefab.Prefab);
            ecb.AddComponent(entity, new PlayerAnimatorReference
            {
                Animator = newCompanionGameObject.GetComponent<Animator>()
            });
            //Debug.Log($"animator stworzony dla{entity}");
            ecb.AddComponent(entity, new PlayerTransformReference
            {
                Transform = newCompanionGameObject.GetComponent<Transform>()
            });

        }
        // animation
        foreach (var (animatorReference, speed, entity) in
            SystemAPI.Query<PlayerAnimatorReference, PlayerSpeed>().WithEntityAccess())
        {
            //Debug.Log($"Entity: {entity} | Current Speed: {speed.currentSpeed}");
            float normalizedSpeed = 0f;
            if (speed.maxSpeed > 0f)
            {
                normalizedSpeed = math.clamp(speed.currentSpeed / speed.maxSpeed, 0f, 1f);
            }
            animatorReference.Animator.SetFloat("Speed", normalizedSpeed);
        }
        // Destroy
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