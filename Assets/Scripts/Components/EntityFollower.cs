using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
/*
public partial struct EntityFollower : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var ( playerPrefab, localTransform, entity) in
             SystemAPI.Query<PlayerGOPrefab,LocalTransform>()
            .WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        {
            var transform = manager.GetComponentData<LocalTransform>(Target);
            playerPrefab.Prefab.Position = transform.Position;
            playerPrefab.Prefab.Rotation = transform.Rotation;
        }
    }
}
*/