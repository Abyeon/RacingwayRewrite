using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Colors;

namespace RacingwayRewrite.Windows.Tabs;

public class Debug(Plugin plugin) : ITab
{
    public string Name => "Debug";
    
    internal readonly Plugin Plugin = plugin;

    public void Dispose() { }
    
    public void Draw()
    {
        if (ImGui.Button("Show Edit Window"))
        {
            Plugin.ToggleEditUI();
        }

        ImGui.SameLine();
        if (ImGui.Button("Test chat functions"))
        {
            Plugin.Chat.Print("Printing example chats:");
            Plugin.Chat.Print("This message has an icon!", BitmapFontIcon.VentureDeliveryMoogle);
            Plugin.Chat.Error("Error! Too many triggers on screen. Please check");
            Plugin.Chat.Warning("Warning, your route lacks a proper description. Consider adding one!");
        }

        ImGui.SameLine();
        if (ImGui.Button("Print Chat Icons"))
        {
            Plugin.Chat.TestPrintIcons();
        }

        ImGui.Spacing();
        ImGui.Text("Current Address:");

        var address = Plugin.RaceManager.RouteLoader.CurrentAddress;
        if (address != null)
            ImGui.TextColoredWrapped(ImGuiColors.DalamudGrey, address.ToString());
    }
}
