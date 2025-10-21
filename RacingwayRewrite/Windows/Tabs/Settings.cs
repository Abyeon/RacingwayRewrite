using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.ImGuiFontChooserDialog;
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
        DrawSettings();
    }

    private bool settingsChanged = false;
    
    private void DrawSettings()
    {
        Configuration configuration = Plugin.Configuration;
        
        if (settingsChanged)
        {
            ImGui.TextColored(ImGuiColors.DalamudOrange, "You have unsaved changes!");
            ImGui.SameLine();
            if (ImGui.Button("Save Changes"))
            {
                configuration.Save();
                settingsChanged = false;
            }
        }
        
        Ui.CenteredTextWithLine("Main Settings", ImGui.GetColorU32(ImGuiCol.TabActive));
        
        var trackOthers = configuration.TrackOthers;
        if (ImGui.Checkbox("Track Others", ref trackOthers))
        {
            configuration.TrackOthers = trackOthers;
            settingsChanged = true;
        }

        var showOverlay = configuration.ShowOverlay;
        if (ImGui.Checkbox("Show Overlay", ref showOverlay))
        {
            configuration.ShowOverlay = showOverlay;
            settingsChanged = true;
            Plugin.ShowHideOverlay();
        }
        
        Ui.CenteredTextWithLine("Timer Settings", ImGui.GetColorU32(ImGuiCol.TabActive));
        ImGui.Text("Font: ");
        ImGui.SameLine();
        if (ImGui.Button(Plugin.FontManager.FontName))
        {
            DisplayFontSelector();
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset"))
        {
            Plugin.FontManager.ResetFont();
        }
        
        Ui.CenteredTextWithLine("Debug Settings", ImGui.GetColorU32(ImGuiCol.TabActive));
        var debugMode = configuration.DebugMode;
        if (ImGui.Checkbox("Debug Mode", ref debugMode))
        {
            configuration.DebugMode = debugMode;
            settingsChanged = true;
            Plugin.MainWindow.UpdateTabs();
        }
        
        if (debugMode) DebugSettings(configuration);
    }

    private void DebugSettings(Configuration configuration)
    {
        var openWindows = configuration.OpenWindowsOnStartup;
        if (ImGui.Checkbox("Open Windows At Startup", ref openWindows))
        {
            configuration.OpenWindowsOnStartup = openWindows;
            settingsChanged = true;
        }
    }
    
    private void DisplayFontSelector()
    {
        var chooser = SingleFontChooserDialog.CreateAuto((UiBuilder)Plugin.PluginInterface.UiBuilder);

        if (Plugin.Configuration.TimerFont is SingleFontSpec font)
        {
            chooser.SelectedFont = font;
        }
        
        chooser.SelectedFontSpecChanged += Chooser_SelectedFontSpecChanged;
    }

    private void Chooser_SelectedFontSpecChanged(SingleFontSpec font)
    {
        Plugin.Configuration.TimerFont = font;
        Plugin.FontManager.FontHandle = Plugin.Configuration.TimerFont.CreateFontHandle(Plugin.PluginInterface.UiBuilder.FontAtlas);
        Plugin.FontManager.FontName = Plugin.Configuration.TimerFont.ToLocalizedString(System.Globalization.CultureInfo.CurrentCulture.Name);
        settingsChanged = true;
    }
}
