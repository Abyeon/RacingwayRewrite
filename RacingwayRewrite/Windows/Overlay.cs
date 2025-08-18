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
        
        this.Plugin = plugin;
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
            
            foreach (var cube in Plugin.RaceManager.Cubes)
            {
                //drawList.AddCubeFilled(cube, 0x5500FF00);
                drawList.AddCube(cube, 0x5500FF0F);
            }
            
            foreach (var player in Plugin.RaceManager.Players.Values)
            {
                Cube cube = new Cube(player.Position, Plugin.Configuration.Scale, Plugin.Configuration.Rotation);
                //drawList.AddCubeFilled(cube, 0x5500FF00);
                drawList.AddCube(cube, 0x5500FF0F);
            }
        }
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
    }
}
