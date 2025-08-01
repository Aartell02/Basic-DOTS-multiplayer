using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class GameInitialize : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        DefaultConnectAddress = NetworkEndpoint.LoopbackIpv4;
        AutoConnectPort = 7979;
        Debug.Log($"[Game Initialize] Port = 7979");
        return base.Initialize(defaultWorldName);
    }
}
