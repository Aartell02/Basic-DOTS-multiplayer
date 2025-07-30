using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct ChatMessage : IComponentData
{
    [GhostField]
    public FixedString64Bytes Message;

    [GhostField]
    public FixedString32Bytes Sender;
}
