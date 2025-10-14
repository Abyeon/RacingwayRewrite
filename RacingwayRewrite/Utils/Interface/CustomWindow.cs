using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace RacingwayRewrite.Utils.Interface;

public abstract class CustomWindow : Window
{
    protected CustomWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
    }

    protected CustomWindow(string name) : base(name)
    {
    }
    
    private Vector2 padding;
    private bool needsPop = false;

    public override void PreDraw()
    {
        // Push border style
        var borderColor = ImGui.GetColorU32(IsFocused ? ImGuiCol.TitleBgActive : IsOpen ? ImGuiCol.TitleBg : ImGuiCol.TitleBgCollapsed);
        var borderSize = IsFocused ? 2f : ImGui.GetStyle().WindowBorderSize;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, borderSize);
        ImGui.PushStyleColor(ImGuiCol.Border, borderColor);
        
        // Push padding
        padding = ImGui.GetStyle().WindowPadding;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        needsPop = true;
        base.PreDraw();
    }

    public void PreRender()
    {
        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(1);
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

    public override void PostDraw()
    {
        if (needsPop)
        {
            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor();
        }
        base.PostDraw();
    }

    public override void Draw()
    {
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
        needsPop = false;
        
        try
        {
            PreRender();

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
    
    private static uint GetAlphaAdjustedColorU32(ImGuiCol idx)
    {
        var bg = ImGui.GetColorU32(ImGuiCol.WindowBg);
        var bgAlpha = bg >> 24;
            
        var color = ImGui.GetColorU32(idx);
        var alpha = color >> 24;
        var alphaDiff = alpha - bgAlpha;
        return color ^ (alphaDiff << 24);
    }
}
