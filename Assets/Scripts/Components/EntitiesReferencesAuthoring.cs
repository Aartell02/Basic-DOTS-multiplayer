#if !UNITY_DISABLE_MANAGED_COMPONENTS
using System;
using Unity.Entities;
using Unity.Mathematics;
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
        AddComponentObject(entity, new PlayerGOPrefab
        {
            Prefab = authoring.playerPrefab
        });

    }
}
public struct EntityReference : IComponentData
{
    public Entity playerReference;
}
public class PlayerGOPrefab : IComponentData
{
    public GameObject Prefab;
}
public class PlayerAnimatorReference : ICleanupComponentData
{
    public Animator Animator;
}

#endif
