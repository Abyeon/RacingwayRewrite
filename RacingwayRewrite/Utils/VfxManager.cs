using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RacingwayRewrite.Utils.Interop;

namespace RacingwayRewrite.Utils;

/// <summary>
/// Highly inspired from Dalamud-VfxEditor. Should be somewhat more convenient, though.
/// </summary>
public class VfxManager : IDisposable
{
    private IClientState ClientState;
    private IFramework Framework;
    public const int MaxVfx = 60;
    
    public readonly LinkedList<BaseVfx> TrackedVfx = [];

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
        for (var item = TrackedVfx.First; item != null;)
        {
            var next = item.Next;
            
            if (!item.Value.Loop && DateTime.UtcNow >= item.Value.Expires)
            {
                item.Value.Dispose();
                //TrackedVfx.Remove(item); no need to remove, disposing should trigger the detour and that will remove this for us
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
        Plugin.Framework.RunOnFrameworkThread(() =>
        {
            // If we hit the max threshold, remove one vfx
            if (TrackedVfx.Count == MaxVfx)
            {
                var first = TrackedVfx.First;
                first?.Value.Dispose();
                TrackedVfx.RemoveFirst();
            }

            TrackedVfx.AddLast(vfx);
        });
    }

    /// <summary>
    /// Dispose all currently tracked vfx.
    /// </summary>
    public void ClearVfx()
    {
        Plugin.Framework.RunOnFrameworkThread(() =>
        {
            for (var item = TrackedVfx.First; item != null;)
            {
                var next = item.Next;
                TrackedVfx.Remove(item);
                item.Value.Dispose();
                item = next;
            }
        });
    }

    public unsafe void InteropRemoved(IntPtr pointer)
    {
        for (var item = TrackedVfx.First; item != null;)
        {
            var next = item.Next;

            if ((IntPtr)item.Value.Vfx == pointer)
            {
                if (item.Value.Loop)
                {
                    item.Value.Refresh();
                }
                else
                {
                    TrackedVfx.Remove(item);
                }
                break;
            }
            
            item = next;
        }
    }
    
    public void Dispose()
    {
        ClearVfx();
        GC.SuppressFinalize(this);
    }
}
