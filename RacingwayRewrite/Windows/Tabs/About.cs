using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;

namespace RacingwayRewrite.Windows.Tabs;

public class About(Plugin plugin) : ITab
{
    public string Name => "About";

    internal readonly Plugin Plugin = plugin;
    
    public void Dispose() { }
    
    public void Draw()
    {
        ImGui.TextColored(ImGuiColors.DalamudRed, $"{Plugin.PluginInterface.Manifest.Name.ToString()} v{Plugin.PluginInterface.Manifest.AssemblyVersion.ToString()}");
        ImGui.SameLine();
        ImGui.Text("by Abyeon");
        
        ImGui.TextColored(ImGuiColors.DalamudGrey, $"Database size on disk: {Plugin.Storage?.FileSize}");
        
        if (Plugin.PluginInterface.Manifest.Changelog != null)
        {
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Text("Changelog: ");
            ImGui.TextWrapped(Plugin.PluginInterface.Manifest.Changelog.ToString());
        }
        
        if (ImGui.Button("GitHub"))
        {
            Util.OpenLink("https://github.com/Abyeon/Racingway");
        }
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip("Wanna make fun of my code? Have a look!");
        }
        ImGui.SameLine();
        if (ImGui.Button("Strange Housing"))
        {
            Util.OpenLink("https://strangehousing.ju.mp/");
        }
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip("Want to experience more jump puzzles? Check out Strange Housing!");
        }
        ImGui.SameLine();
        using (_ = ImRaii.PushFont(UiBuilder.IconFont))
        {
            using (_ = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
            {
                if (ImGui.Button($"{FontAwesomeIcon.Heart.ToIconString()}"))
                {
                    Util.OpenLink("https://ko-fi.com/abyeon");
                }
            }
        }
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip("Support the dev <3");
        }
    }
}
