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
                Vector4 nameColor = ImGuiColors.DalamudWhite;
                if (Plugin.RaceManager.RouteLoader.LoadedRoutes.Exists(x => x.Id == routeInfo.Id))
                    nameColor = ImGuiColors.ParsedBlue;
                
                ImGui.TextColored(nameColor, routeInfo.Name);
                // ImGui.TextColored(ImGuiColors.DalamudYellow, routeInfo.Id.ToString());

                ImGui.TextColored(ImGuiColors.DalamudGrey, $"by {routeInfo.Author}");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey3, $"@ {routeInfo.Address.ReadableName}");

                if (!string.IsNullOrEmpty(routeInfo.Description))
                    ImGui.TextColoredWrapped(ImGuiColors.DalamudGrey2, routeInfo.Description);
                
                // ImGui.TextColored(new Vector4(1f, 0.75f, 0f, 1f), "Name One");
                // ImGui.SameLine();
                // ImGui.TextColored(ImGuiColors.DalamudGrey, "1:28.006");
                // ImGui.SameLine();
                // ImGui.TextColored(new Vector4(0.75f, 0.75f, 0.75f, 1f), "Name Two");
                // ImGui.SameLine();
                // ImGui.TextColored(ImGuiColors.DalamudGrey, "1:28.006");
                // ImGui.SameLine();
                // ImGui.TextColored(new Vector4(0.808f, 0.537f, 0.275f, 1f), "Name Three");
                // ImGui.SameLine();
                // ImGui.TextColored(ImGuiColors.DalamudGrey, "1:28.006");
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseClicked(ImGuiMouseButton.Right))
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
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.PopupRounding, 5);
        using var popup = ImRaii.Popup("Explore Context Menu");
        if (!popup.Success) return;

        if (Plugin.LifestreamIpcHandler.ExecuteCommand.HasAction)
        {
            if (ImGui.Selectable("Teleport with Lifestream"))
            {
                routeInfo.Address.TeleportWithLifestream();
                ImGui.CloseCurrentPopup();
            }
        }
        
        if (ImGui.Selectable("Copy Address"))
        {
            ImGui.SetClipboardText(routeInfo.Address.ReadableName);
            ImGui.CloseCurrentPopup();
        }

        if (ImGui.Selectable("Export to clipboard"))
        {
            var data = Convert.ToBase64String(routeInfo.PackedRoute);
            ImGui.SetClipboardText(data);
            ImGui.CloseCurrentPopup();
        }
        
        if (Plugin.RaceManager.RouteLoader.CurrentAddress == routeInfo.Address)
        {
            if (ImGui.Selectable("Open in editor"))
            {
                Plugin.RaceManager.RouteLoader.SelectedRoute = Plugin.RaceManager.RouteLoader.LoadedRoutes.FindIndex(x => x.Id == routeInfo.Id);
                Plugin.EditWindow.IsOpen = true;
                
                ImGui.CloseCurrentPopup();
            }
        }
        
        if (Ui.CtrlSelectable("Delete Route", "Ctrl+click to delete this route."))
        {
            Plugin.Storage?.DeleteRoute(routeInfo.Id);
            ImGui.CloseCurrentPopup();
        }
    }
}
