using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct SendChatSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonRW<ChatInputBuffer>(out var inputBufferRW)) return;

        if (inputBufferRW.ValueRO.Messages.Length == 0) return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var netId = SystemAPI.GetSingleton<NetworkId>().Value;
        var senderName = new FixedString32Bytes($"Player{netId}");

        foreach (var msg in inputBufferRW.ValueRO.Messages)
        {
            var e = ecb.CreateEntity();
            ecb.AddComponent(e, new ChatMessage
            {
                Message = msg,
                Sender = senderName
            });
        }

        inputBufferRW.ValueRW.Messages.Clear();
        ecb.Playback(state.EntityManager);
    }

}

// Bufor tymczasowy, dodawany do World jako singleton
public struct ChatInputBuffer : IComponentData
{
    public NativeList<FixedString64Bytes> Messages;
}
[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ReceiveChatSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (msg, entity) in SystemAPI.Query<ChatMessage>().WithEntityAccess())
        {
            // broadcast - mo¿esz tu dodaæ logikê filtrowania lub autoryzacji
            ecb.AddComponent<ChatMessage>(entity); // zostawiamy komponent, by by³ synchronizowany
        }

        ecb.Playback(state.EntityManager);
    }
}
