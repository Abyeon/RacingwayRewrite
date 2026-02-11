using System;
using System.Text;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Group;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;
using SharpDX.Direct2D1;

namespace RacingwayRewrite.Utils.Interop.Structs;

public unsafe class SharedGroupLayoutFunctions
{
    public delegate SharedGroupLayoutInstance* CtorDelegate(SharedGroupLayoutInstance* self);
    public delegate byte LoadSgbDelegate(SharedGroupLayoutInstance* self, LayerManager** creator, byte* pathBytes);
    public delegate LayerManager* GetPreferredLayerManagerDelegate(LayoutManager* layoutManager);
    
    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 33 F6 C7 41 ?? ?? ?? ?? ?? 48 8D 05 ?? ?? ?? ?? 48 89 71 ?? ?? ?? ?? 48 8B F9 48 89 71 ?? 89 71 ?? 48 89 71 ?? C7 41 ?? ?? ?? ?? ?? 48 83 C1 ?? E8 ?? ?? ?? ?? 48 89 77")]
    public CtorDelegate? CtorInternal = null;
    
    [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 48 8B B1 ?? ?? ?? ?? 45 0F B6 F9")]
    public LoadSgbDelegate? LoadSgbInternal = null;
    
    [Signature("BA ?? ?? ?? ?? E9 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 4C 8B 81")]
    public GetPreferredLayerManagerDelegate? GetPreferredLayerManagerInternal = null;
    
    public SharedGroupLayoutFunctions()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
    }

    public SharedGroupLayoutInstance* Ctor(SharedGroupLayoutInstance* self)
    {
        if (CtorInternal == null)
            throw new InvalidOperationException("Ctor sig was not found!");
        
        return CtorInternal(self);
    }

    public byte LoadSgb(SharedGroupLayoutInstance* self, ref LayerManager* creator, string path)
    {
        if (LoadSgbInternal == null)
            throw new InvalidOperationException("LoadSgb sig was not found!");

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(path + "\0");
            fixed (byte* pathPtr = bytes)
            {
                fixed (LayerManager** creatorPtr = &creator)
                {
                    return LoadSgbInternal(self, creatorPtr, pathPtr);
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, $"Error loading sgb {path}");
        }

        creator = null;
        return 0;
    }

    public LayerManager* GetPreferredLayerManager(LayoutManager* self)
    {
        if (GetPreferredLayerManagerInternal == null)
            throw new InvalidOperationException("GetPreferredLayerManager sig was not found!");
        
        return GetPreferredLayerManagerInternal(self);
    }
}
