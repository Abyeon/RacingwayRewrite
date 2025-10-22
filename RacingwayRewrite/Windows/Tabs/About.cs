using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Dalamud.Utility.Numerics;
using RacingwayRewrite.Utils;
using RacingwayRewrite.Utils.Interface;

namespace RacingwayRewrite.Windows.Tabs;

public class About(Plugin plugin) : ITab
{
    public string Name => "About";

    internal readonly Plugin Plugin = plugin;
    
    public void Dispose() { }
    public void OnClose() { }

    public void Draw()
    {
        ImGui.Dummy(new Vector2(0, 10));
        Ui.CenteredTextWithLine($"{Plugin.PluginInterface.Manifest.Name}", ImGui.GetColorU32(ImGuiCol.TabActive));

        var version = $"v{Plugin.PluginInterface.Manifest.AssemblyVersion.ToString()} made by {Plugin.PluginInterface.Manifest.Author}";
        ImGuiHelpers.CenterCursorForText(version);
        ImGui.TextColored(ImGuiColors.DalamudRed, $"v{Plugin.PluginInterface.Manifest.AssemblyVersion.ToString()}");
        ImGui.SameLine();
        ImGui.Text($"made by {Plugin.PluginInterface.Manifest.Author}");
        
        DrawLinks();
        
        ImGui.Dummy(new Vector2(0, 10));
        
        if (Plugin.PluginInterface.Manifest.Changelog != null)
        {
            ImGui.Text("Changelog: ");
            ImGui.TextWrapped(Plugin.PluginInterface.Manifest.Changelog.ToString());
            ImGui.Dummy(new Vector2(0, 10));
        }

        DrawCommands();
        
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.TextColored(ImGuiColors.DalamudGrey, $"Database size on disk: {Plugin.Storage?.FileSize}");
    }

    public static void DrawCommands()
    {
        if (ImGui.CollapsingHeader("Commands"))
        {
            using var _ = ImRaii.PushIndent(5f);
        
            foreach (var command in Plugin.CommandHandler.Commands)
            {
                using (new Ui.Hoverable(command.Name, rounding: 0f, padding: new Vector2(5f, 2f), highlight: true))
                {
                    ImGui.TextColored(ImGuiColors.DalamudGrey, $"/{command.Name.ToLowerInvariant()} → ");
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.DalamudGrey3, $"{command.Description}");
                }
            
                if (Ui.Hovered(command.Name))
                {
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        command.Execute(command.Name, string.Empty);
                    }
                }

                ImGuiHelpers.ScaledDummy(1);
            }
        }
    }

    public static void DrawLinks()
    {
        var group = new ImGuiHelpers.HorizontalButtonGroup()
        {
            IsCentered = true,
            Height = ImGui.GetFrameHeight(),
        };
        
        group.Add("Github", () => Util.OpenLink("https://github.com/Abyeon/Racingway"));
        group.Add("Strange Housing", () => Util.OpenLink("https://strangehousing.ju.mp/"));
        group.Add("Ko-Fi", () => Util.OpenLink("https://ko-fi.com/abyeon"));
        group.Draw();
    }
}
