using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Utils.Props;

namespace RacingwayRewrite.Utils.Sgl;

public class PrefabManager : IDisposable
{
    private IClientState ClientState;
    
    public readonly Queue<Prefab> Prefabs = [];

    public PrefabManager(IClientState clientState)
    {
        ClientState = clientState;
        
        ClientState.ZoneInit += ClientStateOnZoneInit;
    }

    private void ClientStateOnZoneInit(ZoneInitEventArgs obj)
    {
        ClearPrefabs();
    }

    public void AddPrefab(Prefab prefab)
    {
        Prefabs.Enqueue(prefab);
    }

    /// <summary>
    /// Remove all currently tracked BgObjects
    /// </summary>
    public void ClearPrefabs()
    {
        while (Prefabs.Count > 0)
        {
            var prefab = Prefabs.Dequeue();
            prefab.Dispose();
        }
    }

    public void Dispose() => ClearPrefabs();
}
