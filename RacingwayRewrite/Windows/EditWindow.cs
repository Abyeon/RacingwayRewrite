using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Windows;

public class EditWindow : Window, IDisposable
{
    private Plugin Plugin;
    
    public EditWindow(Plugin plugin)
        : base("Racingway Editor###HiFellas", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        
        Plugin = plugin;
    }

    public void Dispose() {}

    private string routeNameInputBuf = "";
    public override void Draw()
    {
        // TODO: Find better names.. This sucks lol
        RouteManager manager = Plugin.RaceManager.RouteManager;

        if (Plugin.ClientState.LocalPlayer == null)
        {
            ImGui.Text("Player currently not loaded into an area.");
            return;
        }
        
        if (manager.CurrentAddress == null)
        {
            ImGui.Text("Current zone not detected. Consider loading into a new zone.");
            return;
        }

        if (manager.SelectedRoute == null)
        {
            // Draw route creation button
            if (ImGui.Button("Create New Route"))
                ImGui.OpenPopup("New Route");

            using (var popup = ImRaii.Popup("New Route"))
            {
                if (popup.Success)
                {
                    ImGui.Text("Please input a name for your new route!");
                    ImGui.Separator();

                    ImGui.InputText("##routeNameInput", ref routeNameInputBuf, 64);

                    if (ImGui.Button("Create"))
                    {
                        if (routeNameInputBuf.Length == 0)
                        {
                            Plugin.Chat.Warning("Please enter a name for the new route!");
                        }
                        else
                        {
                            Route newRoute = new Route(routeNameInputBuf, (Address)manager.CurrentAddress);
                            manager.LoadedRoutes.Add(newRoute);
                            routeNameInputBuf = "";
                        
                            ImGui.CloseCurrentPopup();
                        }
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }
            }
            
            // Draw route selection Dropdown
            if (manager.LoadedRoutes.Count > 0)
            {
                ImGui.Text("Select Route:");

                using (var tree = ImRaii.TreeNode("Routes"))
                {
                    if (tree.Success)
                    {
                        uint id = 0;
                        foreach (var route in manager.LoadedRoutes)
                        {
                            if (ImGui.Selectable($"{route.Name}##{id}", route == manager.SelectedRoute))
                            {
                                if (route == manager.SelectedRoute) return;
                                manager.SelectedRoute = route;
                            }
                        
                            id++;
                        }
                    }
                }
            }
        }
        else
        {
            if (ImGui.Button("Stop Editing Route"))
            {
                manager.SelectedRoute = null;
                return;
            }
            
            DrawRouteInfo(manager.SelectedRoute);
        }
    }

    public void DrawRouteInfo(Route route)
    {
        ImGui.Text(route.Name);
        ImGui.Text(route.Description);

        if (ImGui.CollapsingHeader("Behavior Settings"))
        {
            ImGui.Indent();

            bool allowMounts = route.AllowMounts;
            if (ImGui.Checkbox("Allow Mounts", ref allowMounts))
            {
                route.AllowMounts = allowMounts;
            }

            int laps = route.Laps;
            if (ImGui.InputInt("Required Laps", ref laps, 1, 2) && laps > 0)
            {
                route.Laps = laps;
            }
            
            ImGui.Unindent();
        }
    }
}
