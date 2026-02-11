using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Group;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;

namespace RacingwayRewrite.Utils.Sgl;

public unsafe class Prefab : IDisposable
{
    public SharedGroupLayoutInstance* Data;
    public string Path;
    
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Prefab(string path, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null)
    {
        Data = IMemorySpace.GetDefaultSpace()->Malloc<SharedGroupLayoutInstance>();
        Plugin.SharedGroupLayoutFunctions.Ctor(Data);
        
        Plugin.Log.Verbose($"Attempting to create prefab {path} @ {((IntPtr)Data):x8}");
        Path = path;
        
        Position = position ?? Vector3.Zero;
        Rotation = rotation ?? Quaternion.Identity;
        Scale = scale ?? Vector3.One;
        
        Plugin.Framework.RunOnTick(UpdateModel);
    }

    public void UpdateModel()
    {
        var creator = Plugin.SharedGroupLayoutFunctions.GetPreferredLayerManager(LayoutWorld.Instance()->GlobalLayout);
        if (creator == null) return;
        
        var bytes = Encoding.UTF8.GetBytes(Path + "\0");
        
        fixed (byte* ptr = bytes)
        {
            Data->Init(&creator, ptr);
        }

        var t = Data->GetTransformImpl();
        t->Translation = Position;
        t->Rotation = Rotation;
        t->Scale = Scale;

        Data->SetTransformImpl(t);
    }

    public void Dispose()
    {
        Plugin.Log.Verbose($"Disposing prefab {Path}");
        if (Data == null) return;
        
        Data->Deinit();
        Data->Dtor(0);
        
        IMemorySpace.Free(Data);
        
        Data = null;
    }
}
