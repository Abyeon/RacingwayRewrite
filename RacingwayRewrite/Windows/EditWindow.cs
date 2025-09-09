using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using LiteDB;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Windows;

public class EditWindow : Window, IDisposable
{
    internal readonly Plugin Plugin;
    
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
        RouteLoader loader = Plugin.RaceManager.RouteLoader;

        if (Plugin.ClientState.LocalPlayer == null)
        {
            ImGui.Text("Player currently not loaded into an area.");
            return;
        }
        
        if (loader.CurrentAddress == null)
        {
            ImGui.Text("Current zone not detected. Consider loading into a new zone.");
            return;
        }
        
        // Cast to non-nullable to get the readable name.
        Address currentAddress = (Address)loader.CurrentAddress;
        WindowName = $"Racingway Editor - {currentAddress.ReadableName}##HiFellas";

        if (loader.SelectedRoute == null)
        {
            // Draw route creation button
            if (ImGui.Button("Create New Route"))
                ImGui.OpenPopup("New Route");

            using (var popup = ImRaii.Popup("New Route"))
            {
                if (popup.Success)
                {
                    ImGui.Text("Please enter a name for your new route!");
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
                            Route newRoute = new Route(routeNameInputBuf, (Address)loader.CurrentAddress);
                            loader.LoadedRoutes.Add(newRoute);
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
            if (loader.LoadedRoutes.Count > 0)
            {
                ImGui.Text("Select Route:");
                DrawLoadedRoutes(loader);
            }
        }
        else
        {
            DrawRouteInfo(loader);
        }
    }

    public void DrawLoadedRoutes(RouteLoader loader)
    {
        using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(5f, 5f)))
        {
            using (ImRaii.PushColor(ImGuiCol.ChildBg, ImGui.GetColorU32(ImGuiCol.FrameBg)))
            {
                using var child = ImRaii.Child("Routes", Vector2.Zero, true, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysUseWindowPadding);
                if (child.Success)
                {
                    uint id = 0;
                    foreach (var route in loader.LoadedRoutes)
                    {
                        if (ImGui.Selectable($"{route.Name}##{id}", route == loader.SelectedRoute))
                        {
                            if (route == loader.SelectedRoute) return;
                            loader.SelectedRoute = route;
                        }
                        
                        id++;
                    }
                }
            }
        }
    }

    public void DrawRouteInfo(RouteLoader loader)
    {
        if (loader.SelectedRoute == null) return;
        
        if (ImGui.Button("Stop Editing Route"))
        {
            loader.SelectedRoute = null;
            return;
        }
        
        Route route = loader.SelectedRoute;
        
        ImGui.SameLine();
        if (ImGui.Button("Delete")) 
            ImGui.OpenPopup("Delete Route");
        
        DrawRouteDeletionPopup(loader);
        
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
        
        DrawTriggerSettings(route);
    }

    public void DrawTriggerSettings(Route route)
    {
        
    }

    private void DrawRouteDeletionPopup(RouteLoader loader)
    {
        if (loader.SelectedRoute == null) return;
        if (Plugin.Storage == null) return;

        try
        {
            using (var popup = ImRaii.Popup("Delete Route"))
            {
                if (popup.Success)
                {
                    ImGui.Text("Are you sure you want to delete this route?");
                    ImGui.Separator();

                    if (ImGui.Button("Confirm"))
                    {
                        var routeCollection = Plugin.Storage.GetRouteCollection();
                        
                        // If the route exists in DB, delete it
                        var id = loader.SelectedRoute.Id;
                        if (loader.SelectedRoute.Id != null && routeCollection.Exists(x => x.Id == (ObjectId)id!))
                        {
                            routeCollection.Delete(loader.SelectedRoute.Id);
                        }
                        
                        string name = loader.SelectedRoute.Name;

                        loader.LoadedRoutes.Remove(loader.SelectedRoute);
                        loader.SelectedRoute = null;
                        
                        Plugin.Chat.Print($"Deleted {name} from storage.");

                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Chat.Error(ex.Message);
            Plugin.Log.Error(ex.ToString());
        }
    }
}
