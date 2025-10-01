using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using RacingwayRewrite.Storage;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Windows.Tabs;

public class Explore(Plugin plugin) : ITab
{
    public string Name => "Explore";

    public Plugin Plugin = plugin;
    
    public void Dispose() { }
    
    public void Draw()
    {
        if (Plugin.Storage == null) return;

        for (var i = 0; i < Plugin.Storage.RouteCache.Length; i++)
        {
            var routeInfo = Plugin.Storage.RouteCache[i];
            using var id = ImRaii.PushId(i);

            using (new Ui.Hoverable(routeInfo.Name, rounding: 0f, padding: new Vector2(5f, 2f), highlight: true))
            {
                ImGui.Text(routeInfo.Name);

                ImGui.TextColored(ImGuiColors.DalamudGrey, $"by {routeInfo.Author}");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey3, $"@ {routeInfo.Address.ReadableName}");

                if (!string.IsNullOrEmpty(routeInfo.Description))
                    ImGui.TextColoredWrapped(ImGuiColors.DalamudGrey2, routeInfo.Description);
                
                ImGui.TextColored(new Vector4(1f, 0.75f, 0f, 1f), "Name One");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "1:28.006");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.75f, 0.75f, 0.75f, 1f), "Name Two");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "1:28.006");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.808f, 0.537f, 0.275f, 1f), "Name Three");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "1:28.006");
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("Explore Context Menu");
                }
            }

            DrawContextMenu(routeInfo);

            ImGui.Spacing();
        }
    }

    public void DrawContextMenu(RouteInfo routeInfo)
    {
        using var popup = ImRaii.Popup("Explore Context Menu");
        if (!popup.Success) return;

        if (ImGui.Button("Copy Address"))
        {
            ImGui.SetClipboardText(routeInfo.Address.ReadableName);
            ImGui.CloseCurrentPopup();
        }

        if (ImGui.Button("Export to clipboard"))
        {
            var data = Convert.ToBase64String(routeInfo.PackedRoute);
            ImGui.SetClipboardText(data);
            ImGui.CloseCurrentPopup();
        }

        if (ImGui.Button("Delete Route"))
        {
            Plugin.Storage?.DeleteRoute(routeInfo.Id);
            ImGui.CloseCurrentPopup();
        }
        
        if (Plugin.RaceManager.RouteLoader.CurrentAddress == routeInfo.Address)
        {
            if (ImGui.Button("Open in editor"))
            {
                //
                ImGui.CloseCurrentPopup();
            }
        }
    }
}
