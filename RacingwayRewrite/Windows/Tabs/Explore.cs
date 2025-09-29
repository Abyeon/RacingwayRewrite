using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Windows.Tabs;

public class Explore(Plugin plugin) : ITab
{
    public string Name => "Explore";

    public Plugin Plugin = plugin;
    
    public void Dispose() { }
    
    public void Draw()
    {
        if (Plugin.Storage == null) return;
        
        foreach (var routeInfo in Plugin.Storage.RouteCache)
        {
            using (new Ui.Hoverable(routeInfo.Name))
            {
                ImGui.Text(routeInfo.Name);
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedBlue, $"by {routeInfo.Author}");
                ImGui.TextColored(ImGuiColors.DalamudGrey, routeInfo.Address.ReadableName);
                ImGui.TextColoredWrapped(ImGuiColors.DalamudGrey, routeInfo.Description);
            }
            
            ImGui.Spacing();
        }
    }
}
