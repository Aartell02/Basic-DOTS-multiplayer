#if !UNITY_DISABLE_MANAGED_COMPONENTS
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefab;
}
public class EntitiesReferencesBaker : Baker<EntitiesReferencesAuthoring> 
{
    public override void Bake(EntitiesReferencesAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new EntityReference
        {
            playerReference = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic)
        });
    }
}
public struct EntityReference : IComponentData
{
    public Entity playerReference;
}
#endif
