using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Utils.Interop;

namespace RacingwayRewrite.Utils;

/// <summary>
/// Smarter Vfx Manager.. Still uses inter-frame tracking, but whatev
/// </summary>
public class VfxManager : IDisposable
{
    private IClientState ClientState;
    private IFramework Framework;
    public const int MaxVfx = 60;
    
    private readonly LinkedList<BaseVfx> trackedVfx = new();

    public VfxManager(IClientState clientState, IFramework framework)
    {
        ClientState = clientState;
        ClientState.ZoneInit += ClientStateOnZoneInit;
        ClientState.Logout += ClientStateOnLogout;
        
        Framework = framework;
        Framework.Update += FrameworkOnUpdate;
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        for (var item= trackedVfx.First; item != null;)
        {
            var next = item.Next;

            if (!item.Value.Loop && DateTime.UtcNow >= item.Value.Expires)
            {
                item.Value.Dispose();
                trackedVfx.Remove(item);
            }
            else
            {
                item.Value.CheckForRefresh();
            }
            
            item = next;
        }
    }

    private void ClientStateOnLogout(int type, int code)
    {
        ClearVfx();
    }

    private void ClientStateOnZoneInit(ZoneInitEventArgs obj)
    {
        ClearVfx();
    }

    /// <summary>
    /// Add a new Vfx to the game.
    /// </summary>
    /// <param name="vfx"></param>
    public void AddVfx(BaseVfx vfx)
    {
        // If we hit the max threshold, remove one vfx
        if (trackedVfx.Count == MaxVfx)
        {
            var first = trackedVfx.First;
            first?.Value.Dispose();
            trackedVfx.RemoveFirst();
        }

        trackedVfx.AddLast(vfx);
    }

    public void ClearVfx()
    {
        for (var item = trackedVfx.First; item != null;)
        {
            var next = item.Next;
            item.Value.Dispose();
            trackedVfx.Remove(item);
            item = next;
        }
    }
    
    public void Dispose()
    {
        ClearVfx();
        GC.SuppressFinalize(this);
    }
}
