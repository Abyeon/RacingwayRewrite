using System;
using System.Text;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Common.Math;
using RacingwayRewrite.Utils.Interop.Structs;
using VfxData = RacingwayRewrite.Utils.Interop.Structs.VfxData;
using VfxResourceInstance = RacingwayRewrite.Utils.Interop.Structs.VfxResourceInstance;

namespace RacingwayRewrite.Utils.Interop;

// Mostly yoinked from Pictomancy. https://github.com/sourpuh/ffxiv_pictomancy/blob/master/Pictomancy/VfxDraw/VfxFunctions.cs
// Going to move from using their service for these because I don't like their per-frame vfx handling api.
public unsafe class VfxFunctions
{
    public delegate VfxInitData* VfxInitDataCtorDelegate(VfxInitData* self);
    public delegate VfxData* CreateVfxDelegate(byte* path, VfxInitData* init, byte a3, byte a4, float originX, float originY, float originZ, float sizeX, float sizeY, float sizeZ, float angle, float duration, int a13);
    public delegate VfxData* CreateGameObjectVfxDelegate(byte* path, nint target, nint source, float a4, byte a5, ushort a6, byte a7);
    public delegate void DestroyVfxDataDelegate(VfxData* self);
    public delegate long UpdateVfxTransformDelegate(VfxResourceInstance* vfxInstance, Matrix4x4* transform);
    public delegate long UpdateVfxColorDelegate(VfxResourceInstance* vfxInstance, float r, float g, float b, float a);
    public delegate void RotateMatrixDelegate(Matrix4x4* matrix, float rotation);
    
    [Signature("E8 ?? ?? ?? ?? 8D 57 06 48 8D 4C 24 ??")]
    public VfxInitDataCtorDelegate? VfxInitDataCtor = null;
    
    [Signature("E8 ?? ?? ?? ?? 48 8B D8 48 8D 95")]
    public CreateVfxDelegate? CreateVfxInternal = null;
    
    [Signature("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 74 27 B2 01")]
    public CreateGameObjectVfxDelegate? CreateGameObjectVfxInternal = null;
    
    [Signature("E8 ?? ?? ?? ?? 4D 89 A4 DE ?? ?? ?? ??")]
    public DestroyVfxDataDelegate? DestroyVfxInternal = null;
    
    [Signature("E8 ?? ?? ?? ?? EB 19 48 8B 0B")]
    public UpdateVfxTransformDelegate? UpdateVfxTransformInternal = null;
    
    [Signature("E8 ?? ?? ?? ?? 8B 4B F3")]
    public UpdateVfxColorDelegate? UpdateVfxColorInternal = null;
        
    [Signature("E8 ?? ?? ?? ?? 4C 8D 76 20")]
    public RotateMatrixDelegate? RotateMatrixInternal = null;
    

    public VfxFunctions()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
    }

    public VfxData* CreateVfx(string path, Vector3 position, Vector3 size, float rotation)
    {
        if (CreateVfxInternal == null)
            throw new InvalidOperationException("CreateVfx sig was not found!");
        
        if (VfxInitDataCtor == null)
            throw new InvalidOperationException("VfxInitDataCtor sig was not found!");
        
        var pathBytes = Encoding.UTF8.GetBytes(path);

        var init = new VfxInitData();
        VfxInitDataCtor(&init);
        
        fixed (byte* pathPtr = pathBytes)
        {
            var vfx = CreateVfxInternal(pathPtr, &init, 2, 0, position.X, position.Y, position.Z, size.X, size.Y, size.Z, rotation, 1, -1);
            return vfx;
        }
    }

    public void DestroyVfx(VfxData* self)
    {
        if (DestroyVfxInternal == null)
            throw new InvalidOperationException("DestroyVfx sig was not found!");
        
        DestroyVfxInternal(self);
    }

    public VfxData* CreateGameObjectVfx(string path, nint target, nint source)
    {
        if (CreateGameObjectVfxInternal == null)
            throw new InvalidOperationException("CreateGameObjectVfx sig was not found!");
        
        var pathBytes = Encoding.UTF8.GetBytes(path);
        fixed (byte* pathPtr = pathBytes)
        {
            var vfx = CreateGameObjectVfxInternal(pathPtr, target, source, 1, 0, 0, 1);
            return vfx;
        }
    }
}
