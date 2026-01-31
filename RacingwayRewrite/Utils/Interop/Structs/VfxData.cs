using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace RacingwayRewrite.Utils.Interop.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x1E0)]
public unsafe struct VfxData
{
    [FieldOffset(0x1B8)] public VfxResourceInstance* Instance;
}

[StructLayout(LayoutKind.Explicit, Size = 0xC0)]
public struct VfxResourceInstance
{
    [FieldOffset(0x90)] public Vector3 Scale;
    [FieldOffset(0xA0)] public Vector4 Color;
}
