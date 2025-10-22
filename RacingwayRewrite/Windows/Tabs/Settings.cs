using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using RacingwayRewrite.Utils.Interface;

namespace RacingwayRewrite.Windows.Tabs;

public class Settings(Plugin plugin) : ITab
{
    public string Name => "Settings";

    private Plugin Plugin = plugin;
    
    public void Dispose() { }

    public void OnClose()
    {
        if (settingsChanged)
        {
            Plugin.Chat.Warning("Unsaved changes in settings! These will be discarded if you do not save them.");
        }
    }
    
    private bool settingsChanged = false;
    
    public void Draw()
    {
        Configuration configuration = Plugin.Configuration;
        
        var regionMax = ImGui.GetContentRegionAvail();
        var height = settingsChanged ? ImGui.GetFrameHeightWithSpacing() + ImGui.GetTextLineHeightWithSpacing() : ImGui.GetFrameHeightWithSpacing();
        var size = regionMax with { Y = regionMax.Y - height};

        using (var child = ImRaii.Child("SettingsChild", size))
        {
            if (child.Success)
            {
                DrawSettings(configuration);
            }
        }
        
        if (settingsChanged)
        {
            ImGui.TextColored(ImGuiColors.DalamudOrange, "You have unsaved changes!");
        }
        
        if (ImGui.Button("Save"))
        {
            configuration.Save();
            settingsChanged = false;
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Save and Close"))
        {
            configuration.Save();
            settingsChanged = false;
            Plugin.ToggleMainUI();
        }

        ImGui.SameLine();
        if (ImGui.Button("Discard")) // I know this isn't smart.
        {
            DiscardChanges();
        }

        ImGui.SameLine();
        Ui.RightAlignedButton("Reset");
    }

    private void DiscardChanges()
    {
        Plugin.Configuration = Plugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Plugin.FontManager.SetFont(Plugin.Configuration.TimerFont);
        settingsChanged = false;
    }
    
    private void DrawSettings(Configuration configuration)
    {
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
        TimerSettings(configuration);
        
        Ui.CenteredTextWithLine("Debug Settings", ImGui.GetColorU32(ImGuiCol.TabActive));
        DebugSettings(configuration);
    }

    private void TimerSettings(Configuration configuration)
    {
        ImGui.Text("Font:");
        var chooser = Ui.FontChooser(Plugin.FontManager.FontName, configuration.TimerFont, preview: "+00:12.345\n-00:67.890\nRacingway ftw!!!");
        chooser?.ResultTask.ContinueWith(r =>
        {
            if (r.IsCompletedSuccessfully && r.Result != configuration.TimerFont)
            {
                configuration.TimerFont = r.Result;
                settingsChanged = true;
                Plugin.FontManager.FontHandle = Plugin.Configuration.TimerFont.CreateFontHandle(Plugin.PluginInterface.UiBuilder.FontAtlas);
            }
        });
        ImGui.SameLine();
        if (ImGui.Button("Reset##0"))
        {
            Plugin.FontManager.ResetFont();
            settingsChanged = true;
        }
        
        var color = configuration.TimerColor ?? Ui.GetColorVec4(ImGuiCol.Text);
        if (ImGui.ColorEdit4("Text Color", ref color, ImGuiColorEditFlags.NoInputs))
        {
            if (color != Ui.GetColorVec4(ImGuiCol.Text))
            {
                configuration.TimerColor = color;
                settingsChanged = true;
            }
            else
            {
                configuration.TimerColor = null;
            }
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Reset##1"))
        {
            configuration.TimerColor = null;
            settingsChanged = true;
        }
        
        var bgColor = configuration.TimerBackgroundColor ?? Ui.GetColorVec4(ImGuiCol.WindowBg);
        if (ImGui.ColorEdit4("Background Color", ref bgColor, ImGuiColorEditFlags.NoInputs))
        {
            if (color != Ui.GetColorVec4(ImGuiCol.WindowBg))
            {
                configuration.TimerBackgroundColor = bgColor;
                settingsChanged = true;
            }
            else
            {
                configuration.TimerBackgroundColor = null;
            }
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Reset##2"))
        {
            configuration.TimerBackgroundColor = null;
            settingsChanged = true;
        }
    }

    private void DebugSettings(Configuration configuration)
    {
        var debugMode = configuration.DebugMode;
        if (ImGui.Checkbox("Debug Mode", ref debugMode))
        {
            configuration.DebugMode = debugMode;
            settingsChanged = true;
        }

        if (!debugMode) return;
        
        var openWindows = configuration.OpenWindowsOnStartup;
        if (ImGui.Checkbox("Open Windows At Startup", ref openWindows))
        {
            configuration.OpenWindowsOnStartup = openWindows;
            settingsChanged = true;
        }
    }
}
