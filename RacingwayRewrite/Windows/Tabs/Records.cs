using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using RacingwayRewrite.Utils;
using RacingwayRewrite.Utils.Interface;

namespace RacingwayRewrite.Windows.Tabs;

public class Records : ITab
{
    public string Name => "Records";
    
    public void Dispose() { }

    private int seed = new Random().Next();
    
    public void Draw()
    {
        Random rand = new Random(seed);
        
        for (var i = 0; i <= 100; i++)
        {
            using (new Ui.Hoverable(i.ToString(), rounding: 0f, padding: new Vector2(5f, 2f), highlight: true))
            {
                int parse = rand.Next(0, 100);
                double percent = (double)parse / 100;
                ImGui.TextColored(Time.GetParseColor(percent),$"{percent.ToString("P0")} Yoshi'p Sampo");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "Cafe Jump Puzzle");
                ImGui.SameLine();
                ImGui.Text("1:23.456");
            }
            
            ImGuiHelpers.ScaledDummy(1);
        }
    }
}
