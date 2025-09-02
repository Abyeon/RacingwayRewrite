using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Pictomancy;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Race.Collision.Shapes;

namespace RacingwayRewrite.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    
    public MainWindow(Plugin plugin)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }

        if (ImGui.Button("Place Checkpoint") && Plugin.ClientState.LocalPlayer != null)
        {
            Shape shape = new Cube(Plugin.ClientState.LocalPlayer.Position - new Vector3(0, 0.01f, 0), Vector3.One, Vector3.Zero);
            Plugin.RaceManager.Triggers.Add(new Checkpoint(shape));
        }
        
        if (ImGui.Button("Place Fail") && Plugin.ClientState.LocalPlayer != null)
        {
            Shape shape = new Cube(Plugin.ClientState.LocalPlayer.Position - new Vector3(0, 0.01f, 0), Vector3.One, Vector3.Zero);
            Plugin.RaceManager.Triggers.Add(new Fail(shape));
        }

        if (ImGui.Button("Remove All Cubes"))
        {
            Plugin.RaceManager.Triggers.Clear();
        }
        
#if DEBUG
        ImGui.Text("Debug Buttons");
        if (ImGui.Button("Test chat functions"))
        {
            Plugin.Chat.Print("Test print");
            Plugin.Chat.Error("Test error");
            Plugin.Chat.Warning("Test warning");
        }
#endif
    }
}
