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
        ImGui.Text("EDITING!");
    }
}
