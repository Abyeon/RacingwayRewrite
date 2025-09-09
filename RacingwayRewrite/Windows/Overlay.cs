using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Pictomancy;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Collision.Shapes;
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

        using var drawList = PictoService.Draw();
        if (drawList == null) return;
        
#if DEBUG
        foreach (var player in Plugin.RaceManager.Players.Values)
        {
            drawList.AddDot(player.Position, 2f, 0xFF00FF0F);
                
            Vector3 basePos = new Vector3(0, 0, 1);
            Matrix4x4 rotation = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, player.Rotation);
            Matrix4x4 translation = Matrix4x4.CreateTranslation(player.Position);
            Matrix4x4 transform = rotation * translation;
            Vector3 transformed = Vector3.Transform(basePos, transform);
                
            drawList.AddPathLine(player.Position, transformed, 0xFF0000FF);
                
            Vector3 velocity = Vector3.Normalize(player.Velocity);
                
            drawList.AddPathLine(player.Position, player.Position + velocity, 0xFFFF0000);
        }
#endif

        if (Plugin.RaceManager.RouteLoader.SelectedRoute == null) return;
            
        Route route = Plugin.RaceManager.RouteLoader.SelectedRoute;

        foreach (var trigger in route.Triggers)
        {
            // Manipulate selected trigger
            if (trigger == Plugin.RaceManager.RouteLoader.SelectedTrigger)
            {
                Transform tmpTransform = trigger.Shape.Transform;
                if (DrawExtensions.Manipulate(ref tmpTransform, 0.05f, "RacingwayManipulate"))
                {
                    trigger.Shape.Transform = tmpTransform;
                }
            }
                
            if (trigger.Shape.GetType() == typeof(Cube))
                drawList.AddCubeFilled((Cube)trigger.Shape, trigger.Color);
        }
    }

    public void Dispose()
    {
        io = null;
    }
}
