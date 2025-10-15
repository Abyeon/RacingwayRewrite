using System;
using System.Globalization;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility.Numerics;

namespace RacingwayRewrite.Utils.Interface;

public abstract class CustomWindow : Window
{
    protected CustomWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false)
        : base(name, flags, forceMainWindow)
    {
        AllowClickthrough = false;
        AllowPinning = false;
        
        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Thumbtack,
            IconOffset = new Vector2(2.5f, 1),
            Click = _ =>
            {
                var pinned = (Flags & ImGuiWindowFlags.NoMove) == ImGuiWindowFlags.NoMove;
                if (!pinned) Flags |= ImGuiWindowFlags.NoMove;
                else Flags &= ~(ImGuiWindowFlags.NoMove);
            }
        });
    }

    private Vector2 padding;
    private bool needsPop = false;

    public override void PreDraw()
    {
        // Get the current title bar color
        var index = IsFocused ? ImGuiCol.TitleBgActive :
                    IsOpen ? ImGuiCol.TitleBg : ImGuiCol.TitleBgCollapsed;
        
        var vec4 = Ui.GetColorVec4(index);
        if (IsFocused) vec4.W = 1; // Make titlebar opaque if the window is focused.
        var titleCol = vec4.ToByteColor().RGBA;
        
        // Re-assign title bar color
        ImGui.PushStyleColor(index, titleCol);
        
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
            var color = ImGui.GetColorU32(ImGuiCol.TitleBgActive);
            var color1 = ImGui.GetColorU32(ImGuiCol.WindowBg, 0U);
            
            var size = new Vector2
            {
                X = ImGui.GetWindowSize().X + 5f,
                Y = SizeConstraints.HasValue ? SizeConstraints.Value.MinimumSize.Y * 0.5f : ImGui.GetWindowSize().Y * 0.5f
            };

            var noDeco = (Flags & ImGuiWindowFlags.NoDecoration) == ImGuiWindowFlags.NoDecoration;
            
            var position = new Vector2
            {
                X = ImGui.GetWindowPos().X - 5f,
                Y = ImGui.GetWindowPos().Y + (noDeco ? 0 : ImGui.GetFrameHeight())
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
}
