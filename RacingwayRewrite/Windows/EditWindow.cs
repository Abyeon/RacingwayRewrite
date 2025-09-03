using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

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

    public override void Draw()
    {
        ImGui.Text("Route Name Here");
        ImGui.Text("Route Description Here");

        if (ImGui.CollapsingHeader("Behavior Settings"))
        {
            ImGui.Indent();
            DrawBehaviorSettings();
            ImGui.Unindent();
        }
    }

    public void DrawBehaviorSettings()
    {
        bool allowMounts = false;
        if (ImGui.Checkbox("Allow Mounts", ref allowMounts))
        {
            // Handle this later
        }

        int laps = 1;
        if (ImGui.InputInt("Required Laps", ref laps, 1, 2) && laps > 0)
        {
            // Handle this later
        }
    }
}
