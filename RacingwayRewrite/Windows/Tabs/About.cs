using Dalamud.Bindings.ImGui;

namespace RacingwayRewrite.Windows.Tabs;

public class About(Plugin plugin) : ITab
{
    public string Name => "About";

    internal readonly Plugin Plugin = plugin;
    
    public void Dispose() { }
    
    public void Draw()
    {
        ImGui.Text("RACINGWAY OMG!");
    }
}
