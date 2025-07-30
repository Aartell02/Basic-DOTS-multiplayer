using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

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
[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct PlayerInitializationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (physicMass, shouldInitialize) in SystemAPI.Query<RefRW<PhysicsMass>, EnabledRefRW<InitializePlayerTag>>())
        {
            Debug.Log($"PHYSICS UPDATE");
            physicMass.ValueRW.InverseInertia = float3.zero;
            shouldInitialize.ValueRW = false;
        }

    }
}