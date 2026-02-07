using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Utils.Interop;

namespace RacingwayRewrite.Utils.Props;

public class PropManager : IDisposable
{
    private IClientState ClientState;
    
    public readonly Queue<Prop> Props = [];

    public PropManager(IClientState clientState)
    {
        ClientState = clientState;
        
        ClientState.ZoneInit += ClientStateOnZoneInit;
    }

    private void ClientStateOnZoneInit(ZoneInitEventArgs obj)
    {
        ClearProps();
    }

    public void AddProp(Prop prop)
    { 
        Props.Enqueue(prop);
    }

    /// <summary>
    /// Remove all currently tracked BgObjects
    /// </summary>
    public void ClearProps()
    {
        while (Props.Count > 0)
        {
            var prop = Props.Dequeue();
            prop.Dispose();
        }
    }

    public void Dispose() => ClearProps();
}
