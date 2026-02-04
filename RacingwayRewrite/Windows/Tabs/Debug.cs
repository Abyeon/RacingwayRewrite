using System;
using System.Globalization;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Appearance;
using RacingwayRewrite.Race.Replay;
using RacingwayRewrite.Utils.Interop;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace RacingwayRewrite.Windows.Tabs;

public class Debug(Plugin plugin) : ITab
{
    public string Name => "Debug";
    
    internal readonly Plugin Plugin = plugin;

    public void Dispose() { }
    public void OnClose() { }
    
    public void Draw()
    {
        if (ImGui.Button("Spawn VFX"))
        {
            if (Plugin.ObjectTable.LocalPlayer != null)
            {
                var player = Plugin.ObjectTable.LocalPlayer;
                //Plugin.VfxManager2.AddVfx(new Vfx("vfx/common/eff/itm_tape_01c.avfx", player.Position, Vector3.One, 0f));
                Plugin.VfxManager.AddVfx(new ObjectVfx("vfx/common/eff/itm_tape_01c.avfx", player, player, TimeSpan.FromSeconds(3)));
            }
        }
        
        if (ImGui.Button("Show Edit Window"))
        {
            Plugin.ToggleEditUI();
        }
        
        if (ImGui.Button("Show Timer Window"))
        {
            Plugin.ToggleTimerWindow();
        }
        
        if (ImGui.Button("Test chat functions"))
        {
            Plugin.Chat.Print("Printing example chats:");
            Plugin.Chat.Print("This message has an icon!", BitmapFontIcon.VentureDeliveryMoogle);
            Plugin.Chat.Error("Error! Too many triggers on screen. Please check");
            Plugin.Chat.Warning("Warning, your route lacks a proper description. Consider adding one!");
        }
        
        if (ImGui.Button("Print Chat Icons"))
        {
            Plugin.Chat.TestPrintIcons();
        }

        ImGui.Spacing();
        ImGui.Text("Current Address:");

        var address = Plugin.RaceManager.RouteLoader.CurrentAddress;
        if (address != null)
            ImGui.TextColoredWrapped(ImGuiColors.DalamudGrey, address.ToString());

        if (ImGui.Button("Return to door"))
        {
            Plugin.TerritoryTools.MoveToEntry();
        }
        
        DrawCharacterData();
    }

    private uint indexToEdit = 0;
    private ushort animToPlay = 0;

    private unsafe void ResetHighlight()
    {
        foreach (var obj in Plugin.ObjectTable.ClientObjects)
        {
            var battleChara = (BattleChara*)obj.Address;
            battleChara->Highlight(ObjectHighlightColor.None);
        }
    }

    private bool tpToPlayer = false;
    private PlayerAppearance? appearance;

    public unsafe void DrawCharacterData()
    { 
        if (Plugin.ObjectTable.LocalPlayer == null) return;
        var player = (BattleChara*)Plugin.ObjectTable.LocalPlayer.Address;

        if (ImGui.Button("Copy Appearance"))
        {
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var toSave = player;
                if (Plugin.TargetManager.Target is IBattleChara target)
                {
                    toSave = (BattleChara*)target.Address;
                }
            
                appearance = new PlayerAppearance(toSave);
            });
        }
        
        if (appearance != null)
        {
            if (ImGui.Button("Spawn Appearance"))
            {
                indexToEdit = Plugin.RaceManager.ActorManager.SpawnWithAppearance(appearance);
                ResetHighlight();
            }
        };
        
        var man  = ClientObjectManager.Instance();
        // if (ImGui.Button("Spawn Actor"))
        // {
        //     indexToEdit = Plugin.RaceManager.ActorManager.ClonePlayer();
        //     ResetHighlight();
        // }

        if (ImGui.Button("Clear Actors"))
        {
            Plugin.RaceManager.ActorManager.ClearActors();
            indexToEdit = 0;
            ResetHighlight();
        }

        if (ImGui.Button("Reset Index"))
        {
            indexToEdit = 0;
            ResetHighlight();
        }

        if (ImGui.InputUInt("Index", ref indexToEdit, 1, 1))
        {
            ResetHighlight();
        }

        var character = (BattleChara*)man->GetObjectByIndex((ushort)indexToEdit);
        if (character == null) return;
        
        character->Highlight(ObjectHighlightColor.Blue);
        
        var data = new ActorData(character);
        var playerData = new ActorData(player);
        
        if (ImGui.Button("TP To Player"))
        {
            character->SetPosition(player->Position.X, player->Position.Y, player->Position.Z);
        }

        ImGui.Checkbox("Tp To Player", ref tpToPlayer);
        if (tpToPlayer)
        {
            character->SetPosition(player->Position.X, player->Position.Y, player->Position.Z);
            character->SetRotation(player->Rotation);
        }
        
        ImGui.TextWrapped(data.Position.ToString());
        ImGui.TextWrapped(data.Yaw.ToString(CultureInfo.InvariantCulture));
        
        ImGui.InputUShort("Anim To Play", ref animToPlay);
        
        if (ImGui.Button("Play animation"))
        {
            character->SetMode(CharacterModes.AnimLock, 0);
            character->Timeline.BaseOverride = animToPlay;
            character->Timeline.TimelineSequencer.PlayTimeline(animToPlay);
        }

        var alpha = character->Alpha;
        if (ImGui.SliderFloat("Change Alpha", ref alpha, 0f, 1f))
        {
            character->Alpha = alpha;
        }
        
        for (var i = 0; i < player->Timeline.TimelineSequencer.TimelineIds.Length; i++)
        {
            var id= player->Timeline.TimelineSequencer.TimelineIds[i];
            var speed = player->Timeline.TimelineSequencer.TimelineSpeeds[i];
            
            ImGui.TextWrapped(id.ToString() + " " +  speed.ToString(CultureInfo.InvariantCulture));
        }
    }
}
