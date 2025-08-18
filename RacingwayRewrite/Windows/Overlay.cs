using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Pictomancy;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Windows;

public class Overlay : Window, IDisposable
{
    private Plugin Plugin;
    private ImGuiIOPtr io;
    
    public Overlay(Plugin plugin) : base("###RacingwayOverlay")
    {
        Flags = ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoDocking
                | ImGuiWindowFlags.NoNavFocus
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoFocusOnAppearing;
        
        Plugin = plugin;
    }
    
    public override void Draw()
    {
        if (Plugin.ClientState.LocalPlayer == null) return;
        
        io = ImGui.GetIO();
        
        ImGuiHelpers.SetWindowPosRelativeMainViewport("###RacingwayOverlay", new Vector2(0, 0));
        ImGui.SetWindowSize(io.DisplaySize);

        if (!Plugin.Configuration.ShowDebug) return;

        using (var drawList = PictoService.Draw())
        {
            if (drawList == null) return;
            
            foreach (var trigger in Plugin.RaceManager.Triggers)
            {
                if (trigger.Shape.GetType() == typeof(Cube))
                    drawList.AddCubeFilled((Cube)trigger.Shape, trigger.Color);
            }
            
            foreach (var player in Plugin.RaceManager.Players.Values)
            {
                drawList.AddDot(player.Position, 2f, 0xFF00FF0F);
                drawList.AddPathLine(player.Position, player.Position + (player.Velocity * 15), 0xFFFF0000);
            }
        }
    }

    public void Dispose()
    {
        io = null;
    }
}
