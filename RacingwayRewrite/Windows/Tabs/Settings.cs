using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Utility.Numerics;
using RacingwayRewrite.Utils.Interface;

namespace RacingwayRewrite.Windows.Tabs;

public class Settings(Plugin plugin) : ITab
{
    public string Name => "Settings";

    private Plugin Plugin = plugin;
    
    public void Dispose() { }
    
    public void Draw()
    {
        Configuration configuration = Plugin.Configuration;
        Ui.CenteredTextWithLine("Main Settings", ImGui.GetColorU32(ImGuiCol.TabActive));
        
        var trackOthers = configuration.TrackOthers;
        if (ImGui.Checkbox("Track Others", ref trackOthers))
        {
            configuration.TrackOthers = trackOthers;
            configuration.Save();
        }

        var showOverlay = configuration.ShowOverlay;
        if (ImGui.Checkbox("Show Overlay", ref showOverlay))
        {
            configuration.ShowOverlay = showOverlay;
            configuration.Save();
            Plugin.ShowHideOverlay();
        }
        
        var debugMode = configuration.DebugMode;
        if (ImGui.Checkbox("Debug Mode", ref debugMode))
        {
            configuration.DebugMode = debugMode;
            configuration.Save();
            Plugin.MainWindow.UpdateTabs();
        }
        
        // Draw Debug settings
        if (debugMode) DebugSettings(configuration);
    }

    private void DebugSettings(Configuration configuration)
    {
        Ui.CenteredTextWithLine("Debug Settings", ImGui.GetColorU32(ImGuiCol.TabActive));
        var openWindows = configuration.OpenWindowsOnStartup;
        if (ImGui.Checkbox("Open Windows At Startup", ref openWindows))
        {
            configuration.OpenWindowsOnStartup = openWindows;
            configuration.Save();
        }
    }
}
