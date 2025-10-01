using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Race.Collision.Shapes;
using RacingwayRewrite.Race.Territory;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Windows;

public class EditWindow : Window, IDisposable
{
    internal readonly Plugin Plugin;
    
    public EditWindow(Plugin plugin)
        : base("Racingway Editor###RacingwayEditor", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        
        Plugin = plugin;
    }

    public void Dispose() {}
    
    public override void Draw()
    {
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
        WindowName = $"Racingway Editor - {currentAddress.ReadableName}###RacingwayEditor";

        if (loader.SelectedRoute == -1)
        {
            // Draw route creation button
            if (ImGui.Button("Create New Route"))
                ImGui.OpenPopup("New Route");


            var name = "";
            if (Ui.AddTextConfirmationPopup("New Route", "Please enter a name for your new route!", ref name, 64))
            {
                if (name.Length == 0)
                {
                    Plugin.Chat.Warning("Please enter a name for the new route!");
                    return;
                }
                
                Route newRoute = new Route(name, Plugin.ClientState.LocalPlayer.Name.ToString(), (Address)loader.CurrentAddress);
                loader.LoadedRoutes.Add(newRoute);
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
                    for (int i = 0; i < loader.LoadedRoutes.Count; i++)
                    {
                        var route = loader.LoadedRoutes[i];
                        
                        if (ImGui.Selectable($"{route.Name}##{id}", i == loader.SelectedRoute))
                        {
                            if (i == loader.SelectedRoute) return;
                            loader.SelectedRoute = i;
                        }
                        
                        id++;
                    }
                }
            }
        }
    }

    public void DrawRouteInfo(RouteLoader loader)
    {
        if (loader.SelectedRoute == -1) return;
        
        Route route = loader.LoadedRoutes[loader.SelectedRoute];
        
        if (ImGui.Button("Save and Stop Editing"))
        {
            if (!route.ValidCheckpoints())
            {
                Plugin.Chat.Warning("This route's checkpoints are not set up correctly!\n" +
                                    "Please check that each checkpoint position increments by one or matches that of another checkpoint (this allows for branching paths.)");
            }
            
            loader.SelectedRoute = -1;
            loader.SelectedTrigger = null;
            loader.SaveRoutes();
            
            return;
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Save")) {
            loader.SaveRoutes();
        }


        ImGui.SameLine();
        if (ImGui.Button("Delete")) 
            ImGui.OpenPopup("Delete Route");
        
        DrawRouteDeletionPopup(loader);
        
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Pen, Vector4.Zero))
        {
            ImGui.OpenPopup("Edit Name");
        }
        
        ImGui.SameLine();
        ImGuiComponents.TextWithLabel("Name", route.Name);
        
        var name = route.Name;
        if (Ui.AddTextConfirmationPopup("Edit Name", "Please enter a new name for the route.", ref name, 64))
        {
            if (name.Length == 0)
            {
                Plugin.Chat.Warning("Please enter a name for the route!");
                return;
            }
            
            route.Name = name;
        }

        ImGui.PushID(1);
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Pen, Vector4.Zero))
        {
            ImGui.OpenPopup("Edit Author Name");
        }

        ImGui.SameLine();
        ImGui.Text("Author: " + route.Author);
        ImGuiComponents.HelpMarker("It is recommended that you list the author of the jump puzzle you are making this route for.");
        
        var author = route.Author;
        if (Ui.AddTextConfirmationPopup("Edit Author Name", "Please name the author of the puzzle you are creating a route for!", ref author, 64))
        {
            route.Author = author;
        }
        
        ImGui.PushID(2); // ImGui hates me if this isn't here.
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Pen, Vector4.Zero))
        {
            ImGui.OpenPopup("Edit Description");
        }
        
        ImGui.SameLine();
        ImGui.TextWrapped("Description: " + (route.Description.Length > 0 ? route.Description : "No description for this route."));
        
        
        var desc = route.Description;
        if (Ui.AddTextConfirmationPopup("Edit Description", "Please enter a new description for the route.", ref desc, 256, true))
        {
            route.Description = desc;
        }
        

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
        
        DrawTriggerSettings(loader);
    }

    public void DrawTriggerSettings(RouteLoader loader)
    {
        if (Plugin.ClientState.LocalPlayer == null) return;
        if (loader.SelectedRoute == -1) return;
        
        Route route = loader.LoadedRoutes[loader.SelectedRoute];
        
        if (ImGui.Button("Add Trigger"))
        {
            Shape shape = new Cube(Plugin.ClientState.LocalPlayer.Position - new Vector3(0, 0.01f, 0), Vector3.One, Vector3.Zero);
            route.Triggers.Add(new Checkpoint(shape));
        }

        if (loader.SelectedTrigger != null)
        {
            if (ImGui.Button("Stop Editing"))
            {
                loader.SelectedTrigger = null;
            }
            
            ImGui.SameLine();
            if (ImGuiComponents.IconButton("RacingwayTranslate", FontAwesomeIcon.ArrowsUpDownLeftRight))
            {
                DrawExtensions.Operation = ImGuizmoOperation.Translate;
            }
        
            ImGui.SameLine();
            if (ImGuiComponents.IconButton("RacingwayRotate", FontAwesomeIcon.ArrowsSpin))
            {
                DrawExtensions.Operation = ImGuizmoOperation.Rotate;
            }
        
            ImGui.SameLine();
            if (ImGuiComponents.IconButton("RacingwayScale", FontAwesomeIcon.ExpandAlt))
            {
                DrawExtensions.Operation = ImGuizmoOperation.Scale;
            }
        }

        using var child = ImRaii.Child("RouteTriggersChild",  Vector2.Zero, true, ImGuiWindowFlags.AlwaysAutoResize);
        if (!child.Success) return;
        
        var ctrl = ImGui.GetIO().KeyCtrl;
        
        var id = 0;
        foreach (var trigger in route.Triggers.ToList())
        {
            using (new Ui.Hoverable(id.ToString(), padding: new Vector2(5f, 5f)))
            {
                var transform = trigger.Shape.Transform;
                using var pushedId = ImRaii.PushId(id++);
                    
                if (ImGuiComponents.IconButton("###RaceEditBehavior", FontAwesomeIcon.Cog))
                {
                    ImGui.OpenPopup("Trigger Behavior");
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Edit trigger behavior.");
                }
                    
                ITrigger temp = trigger;
                if (DrawTriggerBehaviourPopup(ref temp))
                {
                    int index = route.Triggers.IndexOf(trigger);
                    route.Triggers[index] = temp;
                }

                ImGui.SameLine();
                if (ImGuiComponents.IconButton("###RaceMove", FontAwesomeIcon.ArrowsToDot))
                {
                    trigger.Shape.Transform.Position = Plugin.ClientState.LocalPlayer.Position - new Vector3(0, 0.01f, 0);
                    Plugin.Chat.Print("Moved trigger to player's feet.");
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Set trigger position to your characters position.");
                }
                    
                if (loader.SelectedTrigger != trigger)
                {
                    ImGui.SameLine();
                    if (ImGuiComponents.IconButton("###RaceGizmo", FontAwesomeIcon.RulerCombined))
                    {
                        loader.SelectedTrigger = trigger;
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Edit using the gizmo");
                    }
                }
                    
                ImGui.SameLine();
                using (_ = ImRaii.Disabled(!ctrl))
                {
                    if (ImGuiComponents.IconButton("###RaceErase", FontAwesomeIcon.Eraser))
                    {
                        route.Triggers.Remove(trigger);
                        continue;
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Ctrl + Click to erase this trigger.");
                    }
                }
                    
                // Draw trigger type selection
                ImGui.SameLine();
                    
                var name = trigger is Checkpoint checkpoint ? $"{trigger.GetType().Name} ({checkpoint.Position})" : trigger.GetType().Name;
                
                if (ImGui.Selectable(name, size: new Vector2(150f, 0)))
                {
                    ImGui.OpenPopup("Trigger Type");
                }
                    
                if (DrawTriggerTypePopup(route, ref temp))
                {
                    int index = route.Triggers.IndexOf(trigger);
                    route.Triggers[index] = temp;
                }
                
                Vector3 pos = transform.Position;
                if (ImGui.DragFloat3("Position", ref pos, 0.05f))
                {
                    transform.Position = pos;
                }
                    
                Vector3 scale = transform.Scale;
                if (ImGui.DragFloat3("Scale", ref scale, 0.05f))
                {
                    transform.Scale = scale;
                }
                    
                Vector3 rot = transform.Rotation;
                if (ImGui.DragFloat3("Rotation", ref rot, 0.1f))
                {
                    transform.Rotation = rot;
                }
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                loader.HoveredTrigger = trigger;
            } else if (loader.HoveredTrigger == trigger)
            {
                loader.HoveredTrigger = null;
            }

            ImGui.Spacing();
            //ImGui.Separator();
        }
    }

    private bool DrawTriggerBehaviourPopup(ref ITrigger trigger)
    {
        using var popup = ImRaii.Popup("Trigger Behavior");
        if (!popup.Success) return false;

        if (trigger is Checkpoint checkpoint)
        {
            var position = checkpoint.Position;
            if (ImGui.InputUInt("Position", ref position) && position > 0)
            {
                checkpoint.Position = position;
                trigger = checkpoint;
                return true;
            }
        }

        if (ImGui.Selectable("Trigger if player inside", trigger.TriggerFlags == Behavior.Always))
        {
            trigger.TriggerFlags = Behavior.Always;
            return true;
        }

        if (ImGui.Selectable("Trigger when on ground", trigger.TriggerFlags == Behavior.OnlyGrounded))
        {
            trigger.TriggerFlags = Behavior.OnlyGrounded;
            return true;
        }

        if (ImGui.Selectable("Trigger when jumping", trigger.TriggerFlags == Behavior.OnlyJumping))
        {
            trigger.TriggerFlags = Behavior.OnlyJumping;
            return true;
        }

        return false;
    }

    private bool DrawTriggerTypePopup(Route route, ref ITrigger trigger)
    {
        using var popup = ImRaii.Popup("Trigger Type");
        if (!popup.Success) return false;

        if (ImGui.Selectable("Start", trigger is Start))
        {
            if (trigger is Start) return false;
            
            if (route.Triggers.Exists(trigger => trigger is Start))
            {
                Plugin.Chat.Warning("Route already contains a start! You cannot have more than one.");
                return false;
            }

            if (route.Triggers.Exists(trigger => trigger is Loop))
            {
                Plugin.Chat.Warning("Cannot add start trigger when there is a loop trigger.");
                return false;
            }

            trigger = new Start(trigger.Shape)
            {
                Route = route
            };
            
            return true;
        }

        if (ImGui.Selectable("Loop", trigger is Loop))
        {
            if (trigger is Loop) return false;

            if (route.Triggers.Exists(trigger => trigger is Loop))
            {
                Plugin.Chat.Warning("Route already contains a loop! You cannot have more than one.");
                return false;
            }

            if (route.Triggers.Exists(trigger => trigger is Start or Finish))
            {
                Plugin.Chat.Warning("Cannot add a loop when there are start or finish triggers.");
                return false;
            }

            trigger = new Loop(trigger.Shape)
            {
                Route = route
            };

            return true;
        }

        if (ImGui.Selectable("Checkpoint", trigger is Checkpoint))
        {
            if (trigger is Checkpoint) return false;
            
            trigger = new Checkpoint(trigger.Shape);
            return true;
        }

        if (ImGui.Selectable("Fail", trigger is Fail))
        {
            if (trigger is Fail) return false;
            
            trigger = new Fail(trigger.Shape);
            return true;
        }

        if (ImGui.Selectable("Finish", trigger is Finish))
        {
            if (trigger is Finish) return false;

            if (route.Triggers.Exists(trigger => trigger is Loop))
            {
                Plugin.Chat.Warning("Cannot add a finish trigger when there is a loop trigger.");
                return false;
            }
            
            trigger = new Finish(trigger.Shape);
            return true;
        }

        return false;
    }

    private void DrawRouteDeletionPopup(RouteLoader loader)
    {
        if (loader.SelectedRoute == -1) return;
        if (Plugin.Storage == null) return;

        try
        {
            using var popup = ImRaii.Popup("Delete Route");
            if (popup.Success)
            {
                ImGui.Text("Are you sure you want to delete this route?");
                ImGui.Separator();

                if (ImGui.Button("Confirm"))
                {
                    var route = loader.LoadedRoutes[loader.SelectedRoute];
                    Plugin.Storage.DeleteRoute(route.Id);
                    loader.SaveRoutes();

                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
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
