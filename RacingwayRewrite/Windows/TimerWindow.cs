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
        : base("Timer##RacingwayRewrite", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        Plugin = plugin;
    }

    public override void PreDraw()
    {
        var color = Plugin.Configuration.TimerBackgroundColor ?? Ui.GetColorVec4(ImGuiCol.WindowBg);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, Plugin.Configuration.TimerRounding);
        base.PreDraw();
    }

    public override void Draw()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        using var _ = new Ui.PushFont(Plugin.FontManager.FontHandle);
        
        var timerText = "00:00.000";
        if (Plugin.ClientState.LocalPlayer is not null)
        {
            var player = Plugin.RaceManager.GetPlayer(Plugin.ClientState.LocalPlayer.EntityId);
            if (player == null) return;

            timerText = Time.PrettyFormatTimeSpan(player.State.Timer.Elapsed);
        }
        
        var color = ImGui.ColorConvertFloat4ToU32(Plugin.Configuration.TimerColor ?? Ui.GetColorVec4(ImGuiCol.Text));
        
        ImGui.TextColored(color, timerText);
    }
}
