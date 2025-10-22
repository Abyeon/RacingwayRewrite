using System.Numerics;
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
        : base("Timer##RacingwayRewrite", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground)
    {
        Plugin = plugin;
    }

    public override void Draw()
    {
        using var _ = new Ui.PushFont(Plugin.FontManager.FontHandle);
        var draw = ImGui.GetWindowDrawList();
        draw.ChannelsSplit(2);
        draw.ChannelsSetCurrent(1);

        var start = ImGui.GetCursorPos() + ImGui.GetWindowPos();
        
        var timerText = "00:00.000";
        if (Plugin.ClientState.LocalPlayer is not null)
        {
            var player = Plugin.RaceManager.GetPlayer(Plugin.ClientState.LocalPlayer.EntityId);
            if (player == null) return;

            timerText = Time.PrettyFormatTimeSpan(player.State.Timer.Elapsed);
        }
        
        ImGui.Text(timerText);
        draw.ChannelsSetCurrent(0);
        
        var end = ImGui.GetCursorPos() + ImGui.GetWindowPos();
        var color = ImGui.GetColorU32(ImGuiCol.WindowBg);
        
        draw.AddRectFilledMultiColor(start, end, 0U, 0U, color, color);
        draw.ChannelsMerge();
    }
}
