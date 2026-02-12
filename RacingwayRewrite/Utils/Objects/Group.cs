using System;
using System.Numerics;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Group;
using FFXIVClientStructs.FFXIV.Client.System.Memory;

namespace RacingwayRewrite.Utils.Objects;

public unsafe class Group : IDisposable
{
    public SharedGroupLayoutInstance* Data;
    public string Path;
    
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Group(string path, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null)
    {
        Data = IMemorySpace.GetDefaultSpace()->Malloc<SharedGroupLayoutInstance>();
        Plugin.SharedGroupLayoutFunctions.Ctor(Data);
        
        Plugin.Log.Verbose($"Attempting to create group {path} @ {((IntPtr)Data):x8}");
        Path = path;
        
        Position = position ?? Vector3.Zero;
        Rotation = rotation ?? Quaternion.Identity;
        Scale = scale ?? Vector3.One;
        
        Plugin.Framework.RunOnTick(SetModel);
    }

    private void SetModel()
    {
        Plugin.SharedGroupLayoutFunctions.LoadSgb(Data, Path);
        
        var t = Data->GetTransformImpl();
        t->Translation = Position;
        t->Rotation = Rotation;
        t->Scale = Scale;

        Data->SetTransformImpl(t);
        Data->SetColliderActive(false);

        var first = Data->Instances.Instances.First;
        var last = Data->Instances.Instances.Last;
        
        if (first != last)
        {
            Plugin.SharedGroupLayoutFunctions.FixGroupChildren(Data);
        }
    }
    
    public void Dispose()
    {
        Plugin.Log.Verbose($"Disposing group {Path}");
        if (Data == null) return;
        
        Data->Deinit();
        Data->Dtor(0);
        
        IMemorySpace.Free(Data);
        
        Data = null;
    }
}
