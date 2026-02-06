using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Network;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Object = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object;

namespace RacingwayRewrite.Utils.Interop.Structs;

// Mostly grabbed from Dalamud-VfxEditor, just restructured
public unsafe class VfxFunctions
{
    // Delegates
    public delegate VfxStruct* StaticVfxCreateDelegate(string path, string pool);
    public delegate VfxStruct* StaticVfxRunDelegate(IntPtr vfx, float a1, uint a2);
    public delegate VfxStruct* StaticVfxRemoveDelegate(IntPtr vfx);
    
    public delegate VfxStruct* ActorVfxCreateDelegate(string path, IntPtr caster, IntPtr target, float a4, char a5, ushort a6, char a7);
    public delegate VfxStruct* ActorVfxRemoveDelegate(IntPtr vfx, char a2);

    public delegate BgObject* BgObjectCreateDelegate(string path, string pool, nint a3);
    public delegate ulong BgObjectRemoveDelegate(Object* obj);
    
    // Funcs
    [Signature("E8 ?? ?? ?? ?? F3 0F 10 35 ?? ?? ?? ?? 48 89 43 08")]
    public StaticVfxCreateDelegate? StaticVfxCreateInternal = null;
    
    [Signature("E8 ?? ?? ?? ?? B0 02 EB 02")]
    public StaticVfxRunDelegate? StaticVfxRunInternal = null;
    
    public StaticVfxRemoveDelegate? StaticVfxRemoveInternal; // Sig applied in ctor
    
    [Signature("40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8")]
    public ActorVfxCreateDelegate? ActorVfxCreateInternal = null;
    
    public ActorVfxRemoveDelegate? ActorVfxRemoveInternal; // Sig applied in ctor

    [Signature("48 89 5C 24 ?? 57 48 83 EC ?? 49 8B D8 48 8B F9 4D 85 C0 75 ?? 48 8B 05")]
    public BgObjectCreateDelegate? BgObjectCreateInternal = null;
    
    // Hooks
    public Hook<StaticVfxRemoveDelegate> StaticVfxRemoveHook;
    public Hook<ActorVfxRemoveDelegate> ActorVfxRemoveHook;
    

    public VfxFunctions()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        
        // Applying sigs in ctor is convenient for making the hooks, apparently. (I'm starting to understand why VfxEdit does things the way they do :P)
        string actorVfxRemoveSig = "0F 11 48 10 48 8D 05";
        var actorVfxRemoveAddressTemp = Plugin.SigScanner.ScanText(actorVfxRemoveSig) + 7;
        var actorVfxRemoveAddress = Marshal.ReadIntPtr(actorVfxRemoveAddressTemp + Marshal.ReadInt32(actorVfxRemoveAddressTemp) + 4);
        ActorVfxRemoveInternal = Marshal.GetDelegateForFunctionPointer<ActorVfxRemoveDelegate>(actorVfxRemoveAddress);

        string staticVfxRemoveSig = "40 53 48 83 EC 20 48 8B D9 48 8B 89 ?? ?? ?? ?? 48 85 C9 74 28 33 D2 E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9";
        var staticVfxRemoveAddress = Plugin.SigScanner.ScanText(staticVfxRemoveSig);
        StaticVfxRemoveInternal = Marshal.GetDelegateForFunctionPointer<StaticVfxRemoveDelegate>(staticVfxRemoveAddress);
        
        // Create hooks
        StaticVfxRemoveHook = Plugin.GameInteropProvider.HookFromAddress<StaticVfxRemoveDelegate>(staticVfxRemoveAddress, StaticVfxRemoveDetour);
        ActorVfxRemoveHook = Plugin.GameInteropProvider.HookFromAddress<ActorVfxRemoveDelegate>(actorVfxRemoveAddress, ActorVfxRemoveDetour);
        
        StaticVfxRemoveHook.Enable();
        ActorVfxRemoveHook.Enable();
    }

    private VfxStruct* StaticVfxRemoveDetour(IntPtr vfx)
    {
        Plugin.VfxManager.InteropRemoved(vfx);
        return StaticVfxRemoveHook.Original(vfx);
    }

    private VfxStruct* ActorVfxRemoveDetour(IntPtr vfx, char a2)
    {
        Plugin.VfxManager.InteropRemoved(vfx);
        return ActorVfxRemoveHook.Original(vfx, a2);
    }

    public BgObject* BgObjectCreate(string path)
    {
        if (BgObjectCreateInternal == null)
            throw new InvalidOperationException($"BgObjectCreate sig was not found!");
        
        return BgObjectCreateInternal(path, "Client.LayoutEngine.Layer.BgPartsLayoutInstance", 0);
        // return BgObjectCreateInternal(path, "Client.System.Scheduler.Instance.BgObject", 0);
    }

    public VfxStruct* StaticVfxCreate(string path)
    {
        if (StaticVfxCreateInternal == null)
            throw new InvalidOperationException($"StaticVfxCreate sig was not found!");
        
        return StaticVfxCreateInternal(path, "Client.System.Scheduler.Instance.VfxObject");
    }

    public VfxStruct* StaticVfxRun(VfxStruct* self)
    {
        if (StaticVfxRunInternal == null)
            throw new InvalidOperationException($"StaticVfxRun sig was not found!");
        
        return StaticVfxRunInternal((IntPtr)self, 0f, 0xFFFFFFFF);
    }

    public void StaticVfxRemove(VfxStruct* self)
    {
        if (StaticVfxRemoveInternal == null)
            throw new InvalidOperationException($"StaticVfxRemove sig was not found!");
        
        StaticVfxRemoveInternal((IntPtr)self);
    }

    public VfxStruct* ActorVfxCreate(string path, IntPtr caster, IntPtr target)
    {
        if (ActorVfxCreateInternal == null)
            throw new InvalidOperationException($"ActorVfxCreate sig was not found!");
        
        return ActorVfxCreateInternal(path, caster, target, -1, (char)0, 0, (char)0);
    }

    public void ActorVfxRemove(VfxStruct* self)
    {
        if (ActorVfxRemoveInternal == null)
            throw new InvalidOperationException($"ActorVfxRemove sig was not found!");
        
        ActorVfxRemoveInternal((IntPtr)self, (char)1);
    }
}
