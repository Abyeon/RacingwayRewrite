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
        
        if (ImGui.Button("Show Edit Window"))
        {
            Plugin.ToggleEditUI();
        }
#if DEBUG
        ImGui.Text("Debug Buttons");
        if (ImGui.Button("Test chat functions"))
        {
            Plugin.Chat.Print("Printing example chats:");
            Plugin.Chat.Error("Error! Too many triggers on screen. Please check");
            Plugin.Chat.Warning("Warning, your route lacks a proper description. Consider adding one!");
        }

        if (ImGui.Button("Print Chat Icons"))
        {
            Plugin.Chat.TestPrintIcons();
        }
#endif
    }
}
