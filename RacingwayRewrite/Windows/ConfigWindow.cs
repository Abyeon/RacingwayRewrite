using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###.
    // This allows for labels to be dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        Size = new Vector2(232, 232);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var trackOthers = Configuration.TrackOthers;
        if (ImGui.Checkbox("Track Others", ref trackOthers))
        {
            Configuration.TrackOthers = trackOthers;
            Configuration.Save();
        }

        var showDebug = Configuration.ShowDebug;
        if (ImGui.Checkbox("Show Debug", ref showDebug))
        {
            Configuration.ShowDebug = showDebug;
            Configuration.Save();
        }

        if (Plugin.RaceManager.SelectedTrigger != -1 && ImGui.Button("Stop Editing"))
        {
            Plugin.RaceManager.SelectedTrigger = -1;
        }

        if (ImGuiComponents.IconButton("RacingwayTranslate", FontAwesomeIcon.ArrowsUpDownLeftRight, ImGuiColors.DalamudGrey, ImGuiColors.ParsedBlue))
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

        int id = 0;
        foreach (var trigger in Plugin.RaceManager.Triggers)
        {
            var transform = trigger.Shape.Transform;
            ImGui.PushID(id++);

            if (Plugin.RaceManager.SelectedTrigger != id - 1 && ImGui.Button("Edit With Gizmo"))
            {
                Plugin.RaceManager.SelectedTrigger = id - 1;
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
    }
}
