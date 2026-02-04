using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using RacingwayRewrite.Race.Appearance;
using RacingwayRewrite.Utils.Interop;
using RacingwayRewrite.Utils.Interop.Structs;

namespace RacingwayRewrite.Utils;

/// <summary>
/// Handles spawning/deleting custom actors from the scene
/// </summary>
public unsafe class ActorManager : IDisposable
{
    private readonly IClientState ClientState;
    private const uint MaxActors = 500;

    public ActorManager(IClientState clientState)
    {
        ClientState = clientState;
        ClientState.ZoneInit += OnZoneInit;
    }

    private void OnZoneInit(ZoneInitEventArgs obj)
    {
        ghosts.Clear();
    }

    private readonly Queue<uint> ghosts = new Queue<uint>();

    public uint SpawnWithAppearance(PlayerAppearance appearance)
    {
        if (Plugin.ObjectTable.LocalPlayer == null) return 0;
        var player = (Character*)Plugin.ObjectTable.LocalPlayer.Address;
        
        var man = ClientObjectManager.Instance();
        
        if (ghosts.Count >= MaxActors)
        {
            var first = ghosts.Dequeue();
            man->DeleteObjectByIndex((ushort)first, 0);
        }
        
        var index = man->CreateBattleCharacter();
        var newActor = (BattleChara*)man->GetObjectByIndex((ushort)index);

        const string name = "Racingway Player";
        for (int i = 0; i < name.Length; i++)
        {
            newActor->Name[i] = (byte)name[i];
        }
        newActor->Name[name.Length] = 0;
        
        newActor->Character.TargetableStatus ^= ObjectTargetableFlags.IsTargetable;
        
        newActor->CharacterSetup.CopyFromCharacter(player, 0);
        newActor->CharacterSetup.CopyFromCharacter((Character*)newActor, CharacterSetupContainer.CopyFlags.None);
        
        *(ActorCustomize*)&newActor->DrawData.CustomizeData = appearance.GetCustomizeData();
        for (var i = 0; i < appearance.EquipmentModels.Length; i++)
        {
            var slot = (DrawDataContainer.EquipmentSlot)i;
            var model = appearance.EquipmentModels[i];
            var modelPtr = &model;
            newActor->DrawData.LoadEquipment(slot, modelPtr, true);
        }

        var firstJob = appearance.WeaponDictionary.Keys.First();
        newActor->ClassJob = firstJob;
        
        //Plugin.VfxManager.AddVfx(new ObjectVfx());
        Plugin.VfxFunctions.CreateGameObjectVfx("vfx/common/eff/wks_e008_c0c.avfx", (nint)newActor, (nint)player);
        // Plugin.Log.Debug($"VFX Pointer: {new IntPtr(vfx->Instance)}");
        
        var firstWeapon = appearance.WeaponDictionary[firstJob];
        foreach (var data in firstWeapon)
        {
            data.LoadToCharacter(newActor);
        }

        newActor->DrawData.GlassesIds[0] = appearance.Facewear;
        newActor->DrawData.IsHatHidden = appearance.IsHatHidden;
        newActor->DrawData.IsVisorToggled = appearance.IsVisorToggled;
        newActor->DrawData.VieraEarsHidden = appearance.IsVieraEarsHidden;
        newActor->DrawData.IsWeaponHidden = appearance.IsWeaponHidden;
        
        newActor->Position = Plugin.ObjectTable.LocalPlayer.Position;
        
        if (newActor->IsReadyToDraw())
        {
            newActor->EnableDraw();
        }
        
        ghosts.Enqueue(index);
        return index;
    }

    public uint ClonePlayer()
    {
        if (Plugin.ObjectTable.LocalPlayer == null) return 0;
        var player = (BattleChara*)Plugin.ObjectTable.LocalPlayer.Address;

        var man  = ClientObjectManager.Instance();
        
        if (ghosts.Count >= MaxActors)
        {
            var first = ghosts.Dequeue();
            man->DeleteObjectByIndex((ushort)first, 0);
        }
        
        var index = man->CreateBattleCharacter();
        var newActor = (BattleChara*)man->GetObjectByIndex((ushort)index);

        const CharacterSetupContainer.CopyFlags flags = CharacterSetupContainer.CopyFlags.ClassJob |
                                                        CharacterSetupContainer.CopyFlags.Position |
                                                        CharacterSetupContainer.CopyFlags.Name |
                                                        CharacterSetupContainer.CopyFlags.Ornament;

        newActor->Character.CharacterSetup.CopyFromCharacter((Character*)player, flags);
        //newActor->DrawData.CustomizeData.BodyType = 2;
        //newActor->DrawData.CustomizeData.Sex = 1;
        
        newActor->Character.TargetableStatus ^= ObjectTargetableFlags.IsTargetable;
            
        if (newActor->IsReadyToDraw())
        {
            newActor->EnableDraw();
        }
            
        ghosts.Enqueue(index);
        return index;
    }

    public void ClearActors()
    {
        var man = ClientObjectManager.Instance();

        try
        {
            while (ghosts.Count > 0)
            {
                var index = ghosts.Dequeue();
                man->DeleteObjectByIndex((ushort)index, 0);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Error while clearing actors.");
        }
    }

    public void Dispose()
    {
        ClearActors();
        
        ClientState.ZoneInit -= OnZoneInit;
    }
}
