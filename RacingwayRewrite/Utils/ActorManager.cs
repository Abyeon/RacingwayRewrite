using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

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

    private Queue<uint> ghosts = new Queue<uint>();

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
