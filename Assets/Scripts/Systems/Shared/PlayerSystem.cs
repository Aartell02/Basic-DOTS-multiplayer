#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

// Input Handling
public partial class PlayerSystem : SystemBase
{
    InputSystem_Actions inputActions;
    InputAction moveAction;
    protected override void OnCreate()
    {
        inputActions = new();  
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
    }
    protected override void OnUpdate()
    {
        float2 move = moveAction.ReadValue<Vector2>();
        Entities.WithAll<GhostOwnerIsLocal>()
            .ForEach((ref PlayerInputVector input) =>
            {
                input.inputVector = move;
            }).Run();
    }
    protected override void OnDestroy()
    {
        inputActions.Disable();
    }
}
// Prefab and Physics Initialization
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct PlayerInitializationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (prefabSelector, ghostOwner, entity)
            in SystemAPI.Query<PlayerGOPrefabSelector, RefRW<GhostOwner>>().WithNone<PlayerGOPrefab>().WithEntityAccess())
        {
            GameObject selectedGO = (ghostOwner.ValueRW.NetworkId % 2 == 0)
            ? prefabSelector.PrefabA
            : prefabSelector.PrefabB;
            ecb.AddComponent(entity, new PlayerGOPrefab
            {
                Prefab = selectedGO.gameObject
            });
        }
        foreach (var (physicMass, physicsVelocity, shouldInitialize,entity) 
            in SystemAPI.Query<RefRW<PhysicsMass>,RefRW<PhysicsVelocity>, EnabledRefRW<InitializePlayerTag>>().WithEntityAccess())
        {
            //Debug.Log($"PHYSICS UPDATE {entity.Index}");
            physicsVelocity.ValueRW.Linear = float3.zero;
            physicsVelocity.ValueRW.Angular = float3.zero;
            physicMass.ValueRW.InverseInertia = float3.zero;
            shouldInitialize.ValueRW = false;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
#endif