using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Pictomancy;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Race.Collision.Shapes;
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
            new Settings(Plugin.Configuration),
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
