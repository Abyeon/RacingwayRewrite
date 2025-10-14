using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace RacingwayRewrite.Utils.Interface;

public abstract class CustomWindow : Window
{
    protected CustomWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false)
        : base(name, flags, forceMainWindow)
    { }

    private Vector2 padding;
    private uint bgCol;
    private bool needsPop = false;

    public override void PreDraw()
    {
        // Get the current title bar color
        var titleCol = ImGui.GetColorU32(IsFocused ? ImGuiCol.TitleBgActive : IsOpen ? ImGuiCol.TitleBg : ImGuiCol.TitleBgCollapsed);
        var alpha = titleCol >> 24;
        
        // Adjust Window alpha to match title bar
        bgCol = ImGui.GetColorU32(ImGuiCol.WindowBg);
        bgCol = (bgCol & 0x00FFFFFF) | (alpha << 24);
        
        ImGui.PushStyleColor(ImGuiCol.WindowBg, bgCol);
        
        // Push border style
        var borderSize = IsFocused ? 2f : ImGui.GetStyle().WindowBorderSize;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, borderSize);
        ImGui.PushStyleColor(ImGuiCol.Border, titleCol);
        
        // Push padding
        padding = ImGui.GetStyle().WindowPadding;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        needsPop = true;
        base.PreDraw();
    }

    protected abstract void Render();

    public void PostRender()
    {
        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSetCurrent(0);
        
        if (IsFocused)
        {
            var sizeConstraints = SizeConstraints;
            
            var color = GetAlphaAdjustedColorU32(ImGuiCol.TitleBgActive);
            var color1 = ImGui.GetColorU32(ImGuiCol.WindowBg, 0);
            
            var size = new Vector2
            {
                X = ImGui.GetWindowSize().X + 5f,
                Y = sizeConstraints.HasValue ? sizeConstraints.Value.MinimumSize.Y * 0.5f : ImGui.GetWindowSize().Y * 0.5f
            };

            var position = new Vector2
            {
                X = ImGui.GetWindowPos().X - 5f,
                Y = ImGui.GetWindowPos().Y + ImGui.GetFrameHeight()
            };
            
            drawList.AddRectFilledMultiColor(position, position + size, color, color, color1, color1);
        }
        
        drawList.ChannelsMerge();
    }

    public override void Draw()
    {
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(2);
        needsPop = false;
        
        try
        {
            var drawList = ImGui.GetWindowDrawList();
            drawList.ChannelsSplit(2);
            drawList.ChannelsSetCurrent(1);

            Vector2 start = ImGui.GetCursorPos() + padding;
            Vector2 end = ImGui.GetWindowSize() - padding;
            Vector2 size = end - start;
            
            ImGui.SetCursorPos(start);
            
            using (var child = ImRaii.Child($"###{WindowName}RenderArea", size))
            {
                if (child.Success)
                {
                    Render();
                }
            }
            
            PostRender();
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex.ToString());
        }
    }
    
    public override void PostDraw()
    {
        if (needsPop)
        {
            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor(2);
        }
        base.PostDraw();
    }
    
    private uint GetAlphaAdjustedColorU32(ImGuiCol idx)
    {
        var bgAlpha = bgCol >> 24;
        
        var color = ImGui.GetColorU32(idx);
        return (color & 0x00FFFFFF) | (bgAlpha << 24);
    }
}
