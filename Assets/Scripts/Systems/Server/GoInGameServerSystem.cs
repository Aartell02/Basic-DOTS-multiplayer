#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.NetCode.Hybrid;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct GoInGameServerSystem : ISystem
{
    private const int MaxPlayers = 2;
    private EntityQuery playerQuery;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GoInGameRequestRpc>();
        state.RequireForUpdate<NetworkId>();
        playerQuery = state.GetEntityQuery(ComponentType.ReadOnly<PlayerTag>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        EntityReference entitiesReferences = SystemAPI.GetSingleton<EntityReference>();

        int currentPlayerCount = playerQuery.CalculateEntityCount();

        foreach (var (receiveRpcCommandRequest, entity)
            in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                .WithAll<GoInGameRequestRpc>().WithEntityAccess())
        {
            if (currentPlayerCount >= MaxPlayers)
            {
                //Debug.LogWarning($"Players limit: ({MaxPlayers}). Entity: {entity} connection denied.");
                entityCommandBuffer.DestroyEntity(entity);
                continue;
            }
            //Debug.Log($"Connected Entity: {entity}");
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            // Spawn the player entity for multiplayer
            Entity playerEntity = entityCommandBuffer.Instantiate(entitiesReferences.playerReference);

            // Set initial position
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(new float3(UnityEngine.Random.Range(-10, +10),+1, 0)));

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