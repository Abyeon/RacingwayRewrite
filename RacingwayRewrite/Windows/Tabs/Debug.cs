using Dalamud.Bindings.ImGui;

namespace RacingwayRewrite.Windows.Tabs;

public class Debug(Plugin plugin) : ITab
{
    public string Name => "Debug";
    
    internal readonly Plugin Plugin = plugin;

    public void Dispose() { }
    
    public void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }
        
        if (ImGui.Button("Show Edit Window"))
        {
            Plugin.ToggleEditUI();
        }
#if DEBUG
        ImGui.Text("Debug Buttons");
        if (ImGui.Button("Test chat functions"))
        {
            Plugin.Chat.Print("Printing example chats:");
            Plugin.Chat.Error("Error! Too many triggers on screen. Please check");
            Plugin.Chat.Warning("Warning, your route lacks a proper description. Consider adding one!");
        }

        if (ImGui.Button("Print Chat Icons"))
        {
            Plugin.Chat.TestPrintIcons();
        }
#endif
    }
}
