using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using MessagePack;
using Pictomancy;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Race.Collision.Shapes;
using RacingwayRewrite.Storage;
using RacingwayRewrite.Windows.Tabs;

namespace RacingwayRewrite.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private ITab[] Tabs;
    
    public MainWindow(Plugin plugin)
        : base("Racingway##RacingwayRewrite", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        
        Plugin = plugin;

        Tabs = [
            new Explore(Plugin),
            new Settings(Plugin),
            new About(Plugin),
            #if DEBUG
            new Debug(Plugin)
            #endif
        ];
    }

    public void Dispose()
    {
        foreach (var tab in Tabs)
        {
            tab.Dispose();
        }
    }

    private string? SelectedTab { get; set; }

    public void SelectTab(string label)
    {
        SelectedTab = label;
    }
    
    // Thanks to Asriel:
    //https://github.com/WorkingRobot/Waitingway/blob/5b97266c2f68f8a6f38d19e1d9a0337973254264/Waitingway/Windows/Settings.cs#L75
    private ImRaii.IEndObject TabItem(string label)
    {
        var isSelected = string.Equals(SelectedTab, label, StringComparison.Ordinal);
        if (isSelected)
        {
            SelectedTab = null;
            var open = true;
            return ImRaii.TabItem(label, ref open, ImGuiTabItemFlags.SetSelected);
        }
        return ImRaii.TabItem(label);
    }
    
    public override void Draw()
    {
        if (ImGui.Button("Open Editor"))
        {
            Plugin.ToggleEditUI();
        }

        ImGui.SameLine();
        if (ImGui.Button("Import Route"))
        {
            try
            {
                if (Plugin.Storage == null) throw new NullReferenceException("Storage is null");
                var packed = Convert.FromBase64String(ImGui.GetClipboardText());
                var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
                var route = MessagePackSerializer.Deserialize<Route>(packed, lz4Options);
                
                Plugin.Storage.SaveRoute(route);
            }
            catch (Exception e)
            {
                Plugin.Chat.Error("Error while trying to import route.");
                Plugin.Log.Error(e.ToString());
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Imports any route that is stored on your clipboard!");
        }
        
        using var tabBar = ImRaii.TabBar("##race-tabs", ImGuiTabBarFlags.None);
        if (tabBar)
        {
            foreach (var tab in Tabs)
            {
                using var child = TabItem(tab.Name);
                if (!child.Success) continue;

                // Make the tab bar sticky by making every tab a child
                using var tabChild = ImRaii.Child($"###{tab.Name}-child");
                if (!tabChild.Success) continue;
                tab.Draw();
            }
        }
    }
}
