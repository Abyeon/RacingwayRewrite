using Dalamud.Bindings.ImGui;

namespace RacingwayRewrite.Windows.Tabs;

public class Settings(Configuration configuration) : ITab
{
    public string Name => "Settings";

    private Configuration Configuration = configuration;
    
    public void Dispose() { }
    
    public void Draw()
    {
        var trackOthers = Configuration.TrackOthers;
        if (ImGui.Checkbox("Track Others", ref trackOthers))
        {
            Configuration.TrackOthers = trackOthers;
            Configuration.Save();
        }

        var showDebug = Configuration.ShowDebug;
        if (ImGui.Checkbox("Show Debug", ref showDebug))
        {
            Configuration.ShowDebug = showDebug;
            Configuration.Save();
        }

        if (Plugin.RaceManager.SelectedTrigger != -1 && ImGui.Button("Stop Editing"))
        {
            Plugin.RaceManager.SelectedTrigger = -1;
        }
    }
}
