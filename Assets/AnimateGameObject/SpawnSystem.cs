/*#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Graphical;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public partial struct SpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ExecuteAnimationWithGameObjects>();
    }

    public void OnUpdate(ref SystemState state)
    { 
        var query = SystemAPI.QueryBuilder()
            .WithAll<NeedsGameObjectInstantiation>()
            .WithNone<PlayerGOInstance>()
            .Build();
        
        var entities = query.ToEntityArray(Allocator.Temp);

        // Get the PlayerGOPrefab from the entity that has EntitiesReferencesAuthoring
        var playerGOPrefabQuery = SystemAPI.QueryBuilder().WithAll<PlayerGOPrefab>().Build();
        var playerGOPrefabEntities = playerGOPrefabQuery.ToEntityArray(Allocator.Temp);
        
        if (playerGOPrefabEntities.Length > 0)
            var playerGOPrefab = state.EntityManager.GetComponentData<PlayerGOPrefab>(playerGOPrefabEntities[0]);

            foreach (var entity in entities)
            {
                var instance = GameObject.Instantiate(playerGOPrefab.Prefab);
                instance.hideFlags |= HideFlags.HideAndDontSave;
                
                state.EntityManager.AddComponentObject(entity, instance.GetComponent<Transform>());
                state.EntityManager.AddComponentObject(entity, instance.GetComponent<Animator>());
                state.EntityManager.AddComponentData(entity, new PlayerGOInstance { Instance = instance });
                
                // Remove the tag since we've instantiated the GameObject
                state.EntityManager.RemoveComponent<NeedsGameObjectInstantiation>(entity);
            }
        }
        
        entities.Dispose();
        playerGOPrefabEntities.Dispose();
    }
}
#endif
*/