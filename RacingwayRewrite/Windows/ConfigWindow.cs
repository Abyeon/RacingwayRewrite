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
        foreach (var trigger in Plugin.RaceManager.Triggers)
        {
            var transform = trigger.Shape.Transform;
            ImGui.PushID(id++);
            
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
            
            Vector3 rot = transform.Rotation * (float)(180/Math.PI);
            if (ImGui.DragFloat3("Rotation", ref rot, 0.1f))
            {
                transform.Rotation = rot * (float)(Math.PI/180);
            }
        }
        
        ImGui.PushID(++id);
        var playerScale = Configuration.Scale;
        if (ImGui.DragFloat3("Scale", ref playerScale, 0.05f))
        {
            Configuration.Scale = playerScale;
            Configuration.Save();
        }
        
        var rotation = Configuration.Rotation * (float)(180/Math.PI);
        if (ImGui.DragFloat3("Rotation", ref rotation, 0.05f))
        {
            Configuration.Rotation = rotation * (float)(Math.PI/180);
            Configuration.Save();
        }
    }
}
