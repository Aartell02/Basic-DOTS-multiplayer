using Unity.Entities;
using UnityEngine;

public class ChatAuthoring : MonoBehaviour
{
    public GameObject prefab;
}
public class ChatBaker : Baker<ChatAuthoring>
{
    public override void Bake(ChatAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new EntityReference
        {
            playerReference = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
        });

    }
}
public struct ChatReference : IComponentData
{
    public Entity chatReference;
}