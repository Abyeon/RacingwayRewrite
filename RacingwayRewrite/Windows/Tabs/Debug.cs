using System;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Newtonsoft.Json.Serialization;
using RacingwayRewrite.Race;

namespace RacingwayRewrite.Windows.Tabs;

public class Debug(Plugin plugin) : ITab
{
    public string Name => "Debug";
    
    internal readonly Plugin Plugin = plugin;

    public void Dispose() { }
    
    public void Draw()
    {
        if (ImGui.Button("Show Edit Window"))
        {
            Plugin.ToggleEditUI();
        }

        ImGui.SameLine();
        if (ImGui.Button("Test chat functions"))
        {
            Plugin.Chat.Print("Printing example chats:");
            Plugin.Chat.Print("This message has an icon!", BitmapFontIcon.VentureDeliveryMoogle);
            Plugin.Chat.Error("Error! Too many triggers on screen. Please check");
            Plugin.Chat.Warning("Warning, your route lacks a proper description. Consider adding one!");
        }

        ImGui.SameLine();
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
        
        //DrawCharacterData();
    }

    public unsafe void DrawCharacterData()
    {
        if (Plugin.ClientState.LocalPlayer == null) return;
        var character = (BattleChara*)Plugin.ClientState.LocalPlayer.Address;
        
        ActorData data = new ActorData(character);
        ImGui.TextWrapped(data.Position.ToString());
        ImGui.TextWrapped(data.Yaw.ToString(CultureInfo.InvariantCulture));
        
        //ImGui.TextWrapped(data.Effects.StatusEffects.ToString());
        foreach (var property in data.Timeline.GetType().GetProperties())
        {
            try
            {
                ImGui.TextWrapped(property.Name);
                //ImGui.SameLine();
                //ImGui.TextWrapped(property.GetValue(data.Timeline)?.ToString());
            }
            catch
            {
                // ignored
            }
        }
    }
}
