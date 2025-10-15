using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using RacingwayRewrite.Utils;
using RacingwayRewrite.Utils.Interface;

namespace RacingwayRewrite.Windows;

public class TimerWindow : Window
{
    private Plugin Plugin;
    
    public TimerWindow(Plugin plugin)
        : base("Timer##RacingwayRewrite", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Plugin = plugin;
    }

    public override void Draw()
    {
        if (Plugin.ClientState.LocalPlayer == null) return;
        
        ImGui.PushFont(ImGui.GetFont());
        
        var player = Plugin.RaceManager.GetPlayer(Plugin.ClientState.LocalPlayer.EntityId);
        if (player == null) return;

        var pretty = Time.PrettyFormatTimeSpan(player.State.Timer.Elapsed);
        ImGui.Text(pretty);
    }
}
