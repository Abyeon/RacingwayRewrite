using System;
using FFXIVClientStructs.FFXIV.Common.Math;
using RacingwayRewrite.Utils.Interop.Structs;

namespace RacingwayRewrite.Utils.Interop;

/// <summary>
/// Rewrite of Picto's VFX class
/// </summary>
public unsafe class Vfx : BaseVfx
{
    public Vector3 Position;
    public Vector3 Size;
    public float Rotation;
    
    public Vfx(string path, Vector3 position, Vector3 size, float rotation)
    {
        if (Plugin.VfxFunctions == null) throw new NullReferenceException("Vfx functions are not initialized");
        
        Position = position;
        Size = size;
        Rotation = rotation;
        
        Plugin.Framework.RunOnFrameworkThread(() =>
        {
            Data = Plugin.VfxFunctions.CreateVfx(path, position, size, rotation);
        });
    }

    protected override void Refresh()
    {
        Data = Plugin.VfxFunctions.CreateVfx(Path, Position, Size, Rotation);
    }
}
