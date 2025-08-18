using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Pictomancy;
using RacingwayRewrite.Race.Collision;

namespace RacingwayRewrite.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
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

        if (ImGui.Button("Place Cube") && Plugin.ClientState.LocalPlayer != null)
        {
            Shape shape = new Cube(Plugin.ClientState.LocalPlayer.Position - new Vector3(0, 0.01f, 0), Vector3.One, Vector3.Zero);
            Plugin.RaceManager.Triggers.Add(new Checkpoint(shape));
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
