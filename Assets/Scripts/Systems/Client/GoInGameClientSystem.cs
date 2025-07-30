using Unity.Entities;
using Unity.NetCode;
using Unity.Burst;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct GoInGameClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach((RefRO<NetworkId> NetworkId, Entity entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);

            Entity rpcEntity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(rpcEntity, new GoInGameRequestRpc());
            entityCommandBuffer.AddComponent(rpcEntity, new SendRpcCommandRequest
            {
                TargetConnection = entity 
            });
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }
}

public struct GoInGameRequestRpc : IRpcCommand
{

}
