using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

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

        int id = 0;
        foreach (var cube in Plugin.RaceManager.Cubes)
        {
            ImGui.PushID(id++);
            ImGui.DragFloat3("Position", ref cube.Transform.Position, 0.05f);
            ImGui.DragFloat3("Scale", ref cube.Transform.Scale, 0.05f);
            ImGui.DragFloat3("Rotation", ref cube.Transform.Rotation, 0.1f);
        }
        
        ImGui.PushID(id++);
        var scale = Configuration.Scale;
        if (ImGui.DragFloat3("Scale", ref scale, 0.05f))
        {
            Configuration.Scale = scale;
            Configuration.Save();
        }
        
        var rotation = Configuration.Rotation;
        if (ImGui.DragFloat3("Rotation", ref rotation, 0.05f))
        {
            Configuration.Rotation = rotation;
            Configuration.Save();
        }
    }
}
