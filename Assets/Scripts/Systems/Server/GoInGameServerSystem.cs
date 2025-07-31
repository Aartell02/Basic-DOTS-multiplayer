#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.NetCode.Hybrid;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GoInGameRequestRpc>();
        state.RequireForUpdate<NetworkId>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        EntityReference entitiesReferences = SystemAPI.GetSingleton<EntityReference>();
        
        foreach (var (receiveRpcCommandRequest, entity)
            in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                .WithAll<GoInGameRequestRpc>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            Debug.Log($"Connected");
            
            // Spawn the player entity for multiplayer
            Entity playerEntity = entityCommandBuffer.Instantiate(entitiesReferences.playerReference);
            Debug.Log($"Spawned player entity: {playerEntity.Index}");
            
            // Set initial position
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(new float3(UnityEngine.Random.Range(-10, +10),+2, 0)));
            
            // Set network ownership
            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            entityCommandBuffer.SetComponent(playerEntity, new GhostOwner
            {
                NetworkId = networkId.Value,
                
            });
            // Add to linked entity group for cleanup
            entityCommandBuffer.AppendToBuffer(receiveRpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup
            {
                Value = playerEntity,
            });
            
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }
}
#endif