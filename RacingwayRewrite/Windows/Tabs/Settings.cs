using Dalamud.Bindings.ImGui;

namespace RacingwayRewrite.Windows.Tabs;

public class Settings(Plugin plugin) : ITab
{
    public string Name => "Settings";

    private Plugin Plugin = plugin;
    
    public void Dispose() { }
    
    public void Draw()
    {
        Configuration configuration = Plugin.Configuration;
        
        var trackOthers = configuration.TrackOthers;
        if (ImGui.Checkbox("Track Others", ref trackOthers))
        {
            configuration.TrackOthers = trackOthers;
            configuration.Save();
        }

        var showDebug = configuration.ShowDebug;
        if (ImGui.Checkbox("Show Overlay", ref showDebug))
        {
            configuration.ShowDebug = showDebug;
            configuration.Save();
            Plugin.ShowHideOverlay();
        }

        if (Plugin.RaceManager.SelectedTrigger != -1 && ImGui.Button("Stop Editing"))
        {
            Plugin.RaceManager.SelectedTrigger = -1;
        }
    }
}
