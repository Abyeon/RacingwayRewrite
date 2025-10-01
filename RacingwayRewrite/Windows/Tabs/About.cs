using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;

namespace RacingwayRewrite.Windows.Tabs;

public class About(Plugin plugin) : ITab
{
    public string Name => "About";

    internal readonly Plugin Plugin = plugin;
    
    public void Dispose() { }
    
    public void Draw()
    {
        ImGui.TextColored(ImGuiColors.DalamudGrey, $"Size on disk: {Plugin.Storage?.FileSize}");
    }
}
