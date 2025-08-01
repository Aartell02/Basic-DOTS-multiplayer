using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EntitiesReferencesBaker;


public class PlayerAuthoring : MonoBehaviour 
{
    public GameObject characterPrefabA;
    public GameObject characterPrefabB;
    public float moveSpeed;
}
public class Baker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent<InitializePlayerTag>(entity);
        AddComponent<PlayerTag>(entity);
        AddComponent<InitializeCameraTargetTag>(entity);
        AddComponent<CameraTarget>(entity);

        /*GameObject selectedGO = (networkId.Value % 2 == 0)
            ? entitiesReferences.characterPrefabA
            : entitiesReferences.characterPrefabB;

        AddComponentObject(entity, new PlayerGOPrefab
        {
            Prefab = authoring.characterPrefabA
        });*/
        AddComponentObject(entity, new PlayerGOPrefabSelector
        {
            PrefabA = authoring.characterPrefabA,
            PrefabB = authoring.characterPrefabB
        });
        AddComponent(entity, new PlayerInputVector
        {
            inputVector = float2.zero
        });


        AddComponent(entity, new PlayerSpeed
        {
            currentSpeed = 0f,
            maxSpeed = 200f,
            acceleration = authoring.moveSpeed
        });
    }
}
public struct PlayerSpeed : IComponentData
{
    [GhostField] public float currentSpeed;
    public float maxSpeed;
    public float acceleration;
}
public struct PlayerInputVector : IInputComponentData
{
    public float2 inputVector;
}
public class PlayerGOPrefab : IComponentData
{
    public GameObject Prefab;
}
public class PlayerGOPrefabSelector : IComponentData
{
    public GameObject PrefabA;
    public GameObject PrefabB;
}
public class PlayerAnimatorReference : ICleanupComponentData
{
    public Animator Animator;
}
public class PlayerTransformReference : IComponentData
{
    public Transform Transform;
}
public struct InitializePlayerTag : IComponentData, IEnableableComponent { }
public struct PlayerTag : IComponentData { }
public struct InitializeCameraTargetTag : IComponentData, IEnableableComponent { }
public struct CameraTarget : IComponentData
{
    public UnityObjectRef<Transform> CameraTransform;
}
