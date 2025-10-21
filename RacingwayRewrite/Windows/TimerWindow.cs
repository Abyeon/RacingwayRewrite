using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using RacingwayRewrite.Utils;
using RacingwayRewrite.Utils.Interface;

namespace RacingwayRewrite.Windows;

public class TimerWindow : Window
{
    private Plugin Plugin;
    
    public TimerWindow(Plugin plugin)
        : base("Timer##RacingwayRewrite", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        Plugin = plugin;
    }

    public override void Draw()
    {
        Plugin.FontManager.PushFont();
        
        var timerText = "00:00.000";
        if (Plugin.ClientState.LocalPlayer is not null)
        {
            var player = Plugin.RaceManager.GetPlayer(Plugin.ClientState.LocalPlayer.EntityId);
            if (player == null) return;

            timerText = Time.PrettyFormatTimeSpan(player.State.Timer.Elapsed);
        }
        
        ImGui.Text(timerText);
        Plugin.FontManager.PopFont();
    }
}
