using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;


public class PlayerAuthoring : MonoBehaviour 
{
    public float moveSpeed;
}
public class Baker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new PlayerMoveSpeed
        {
            moveSpeed = authoring.moveSpeed
        });
        AddComponent(entity, new PlayerInputVector
        {
            inputVector = float2.zero
        });
        AddComponent<InitializePlayerTag>(entity);
        AddComponent<PlayerTag>(entity);
        AddComponent<InitializeCameraTargetTag>(entity);
        AddComponent<CameraTarget>(entity);
        
        // Add physics components that aren't automatically added by RigidbodyBaker
        AddComponent<PhysicsGravityFactor>(entity);
        
        // Set gravity factor
        SetComponent(entity, new PhysicsGravityFactor
        {
            Value = 1f
        });

    }
}
public struct PlayerInputVector : IInputComponentData
{
    public float2 inputVector;
}
public struct PlayerMoveSpeed : IComponentData
{
    public float moveSpeed;
}
public struct InitializePlayerTag : IComponentData, IEnableableComponent { }
public struct PlayerTag : IComponentData { }
public struct InitializeCameraTargetTag : IComponentData { }
public struct CameraTarget : IComponentData
{
    public UnityObjectRef<Transform> CameraTransform;
}