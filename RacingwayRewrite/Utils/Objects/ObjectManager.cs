using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Utils.Vfx;

namespace RacingwayRewrite.Utils.Objects;

/// <summary>
/// Manager for handling spawning and deleting multiple types of objects.
/// </summary>
public class ObjectManager : IDisposable
{
    public List<Model> Models = [];
    public List<Group> Groups = [];
    public List<BaseVfx> Vfx = [];
    
    private IClientState ClientState;
    private IFramework Framework;

    public ObjectManager(IClientState clientState, IFramework framework)
    {
        ClientState = clientState;
        ClientState.ZoneInit += ClientStateOnZoneInit;
        ClientState.Logout += ClientStateOnLogout;
        
        Framework = framework;
        Framework.Update += FrameworkOnUpdate;
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        foreach (var item in Vfx.ToList())
        {
            if (!item.Loop && DateTime.UtcNow >= item.Expires)
            {
                item.Dispose(); // triggers the detour, removing it from the list
            }
            else
            {
                item.CheckForRefresh();
            }
        }
    }

    private void ClientStateOnLogout(int type, int code) => Clear();
    private void ClientStateOnZoneInit(ZoneInitEventArgs obj) => Clear();

    /// <summary>
    /// Takes a path and determines what type of object to spawn.
    /// </summary>
    /// <param name="path">Game path to the file</param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    public void Add(string path, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null)
    {
        var ext = Path.GetExtension(path);
        
        var pos = position ?? Vector3.Zero;
        var rot  = rotation ?? Quaternion.Identity;
        var sca  = scale ?? Vector3.One;

        switch (ext)
        {
            case ".avfx":
                Add(new StaticVfx(path,  pos, sca, 0f));
                break;
            case ".mdl":
                Add(new Model(path, pos, rot, sca));
                break;
            case ".sgb":
                Add(new Group(path, pos, rot, sca));
                break;
            default:
                Plugin.Log.Error($"Unsupported extension {ext}");
                break;
        }
    }

    /// <summary>
    /// Take's .mdl files and spawns BgObjects
    /// </summary>
    /// <param name="model"></param>
    public void Add(Model model)
    {
        Models.Add(model);
    }

    /// <summary>
    /// Takes .sgb files and spawns SharedLayoutGroups
    /// </summary>
    /// <param name="group"></param>
    public void Add(Group group)
    {
        Groups.Add(group);
    }

    /// <summary>
    /// Takes .avfx files and spawns Vfx objects
    /// </summary>
    /// <param name="vfx"></param>
    public void Add(BaseVfx vfx)
    {
        Vfx.Add(vfx);
    }

    public void ClearModels()
    {
        foreach (var model in Models) model.Dispose();
        Models.Clear();
    }

    public void ClearGroups()
    {
        foreach (var group in Groups) group.Dispose();
        Groups.Clear();
    }

    public void ClearVfx()
    {
        foreach (var vfx in Vfx.ToList())
        {
            vfx.Loop = false; // need to disable loop so vfx doesnt get re-enabled after disposing.. oops!
            vfx.Dispose();
        }
        Vfx.Clear();
    }

    public void Clear()
    {
        ClearModels();
        ClearGroups();
        ClearVfx();
    }

    public unsafe void InteropRemoved(IntPtr pointer)
    {
        foreach (var item in Vfx)
        {
            if ((IntPtr)item.Vfx == pointer)
            {
                if (item.Loop)
                {
                    item.Refresh();
                }
                else
                {
                    Vfx.Remove(item);
                }

                break;
            }
        }
    }

    public void Dispose()
    {
        Clear();
    }
}
