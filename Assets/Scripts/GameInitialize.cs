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

        if (!Application.isEditor)
        {
            DefaultListenAddress = NetworkEndpoint.AnyIpv4;
            Debug.Log("[Bootstrap] Server+Client on port 7979");
        }
        else
        {
            Debug.Log("[Bootstrap] Editor Play: Client only");
        }

        return base.Initialize(defaultWorldName);
    }
}
