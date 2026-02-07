using System;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace RacingwayRewrite.Utils.Vfx;

/// <summary>
/// VFX that is not attached to an actor.
/// </summary>
public unsafe class StaticVfx : BaseVfx
{
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;

    public StaticVfx(string path, Vector3 position, Vector3 scale, float rotation, TimeSpan? expiration = null, bool loop = false)
    {
        Plugin.Log.Verbose($"Creating StaticVfx {path}");
        if (Plugin.VfxFunctions == null) throw new NullReferenceException("Vfx functions are not initialized");

        Path = path;
        Position = position;
        Scale = scale;
        Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, rotation);
        Loop = loop;
        Expires = expiration.HasValue ? DateTime.UtcNow + expiration.Value : DateTime.UtcNow + TimeSpan.FromSeconds(5);
        
        try
        {
            Vfx = Plugin.VfxFunctions.StaticVfxCreate(Path);
            Plugin.VfxFunctions.StaticVfxRun(Vfx);
                
            if (!IsValid)
                throw new Exception("Vfx pointer is null");
                
            Vfx->Position = Position;
            Vfx->Scale = Scale;
            Vfx->Rotation = Rotation;
            Vfx->Flags |= 0x2;
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Failed to create Vfx");
        }
    }

    public override void Refresh()
    {
        try
        {
            // if (IsValid) Plugin.VfxFunctions.StaticVfxRemove(Vfx);
            Vfx = Plugin.VfxFunctions.StaticVfxCreate(Path);
            Plugin.VfxFunctions.StaticVfxRun(Vfx);
                
            if (!IsValid)
                throw new Exception("Vfx pointer is null");
                
            Vfx->Position = Position;
            Vfx->Scale = Scale;
            Vfx->Rotation = Rotation;
            Vfx->Flags |= 0x2;
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Failed to create Vfx");
        }
    }

    protected override void Remove()
    {
        Plugin.VfxFunctions.StaticVfxRemove(Vfx);
    }
}
