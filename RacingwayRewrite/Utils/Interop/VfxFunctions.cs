using System;
using System.Text;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;

namespace RacingwayRewrite.Utils.Interop;

public unsafe class VfxFunctions
{
    public delegate VfxData* CreateGameObjectVfxDelegate(byte* path, nint target, nint source, float a4, byte a5, ushort a6, byte a7);
    
    [Signature("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 74 27 B2 01")]
    public readonly CreateGameObjectVfxDelegate? CreateGameObjectVfxInternal = null;

    public VfxFunctions()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
    }

    public VfxData* CreateGameObjectVfx(string path, nint target, nint source)
    {
        if (CreateGameObjectVfxInternal == null)
            throw new InvalidOperationException("CreateGameObjectVfxInternal sig was not found!");
        
        var pathBytes = Encoding.UTF8.GetBytes(path);
        fixed (byte* pathPtr = pathBytes)
        {
            var vfx = CreateGameObjectVfxInternal(pathPtr, target, source, 1, 0, 0, 1);
            return vfx;
        }
    }
}
