using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace RacingwayRewrite.Utils.Interface;

/// <summary>
/// Provides custom ImGui components
/// </summary>
public static class Ui
{
    private static string Buf = "";

    /// <summary>
    /// Adds a popup that can be opened with ImGui.OpenPopup(id)
    /// Only returns true if the confirmation button was clicked.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="description">The text that appears inside the popup</param>
    /// <param name="input">Reference input to be updated</param>
    /// <param name="maxLength">Maximum length for the input</param>
    /// <param name="multiline">Whether the input should be multiline or not.</param>
    /// <returns>True if the input was updated</returns>
    public static bool AddTextConfirmationPopup(
        string id, string description, ref string input, int maxLength = 512, bool multiline = false)
    {
        using var popup = ImRaii.Popup(id);
        if (popup.Success)
        {
            ImGui.PushID(id);
            ImGui.Text(description);
            ImGui.Separator();
            
            if (multiline)
            {
                ImGui.InputTextMultiline("##textInput", ref Buf, maxLength, flags: ImGuiInputTextFlags.NoHorizontalScroll);
            }
            else
            {
                ImGui.InputText("##textInput", ref Buf, maxLength);
            }

            if (ImGui.Button("Confirm"))
            {
                input = Buf;
                Buf = "";
                ImGui.CloseCurrentPopup();
                return true;
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                Buf = "";
                ImGui.CloseCurrentPopup();
                return false;
            }
        }

        return false;
    }

    public static bool CtrlButton(ImU8String label, string hoverLabel = "Hold Ctrl to enable.", Vector2 size = default)
    {
        var ctrl = ImGui.GetIO().KeyCtrl;
        using var _ = ImRaii.Disabled(!ctrl);
        if (ImGui.Button(label, size))
        {
            return true;
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(hoverLabel);
        }

        return false;
    }
    
    public static bool CtrlSelectable(ImU8String label, string hoverLabel = "Hold Ctrl to enable.")
    {
        var ctrl = ImGui.GetIO().KeyCtrl;
        using var _ = ImRaii.Disabled(!ctrl);
        if (ImGui.Selectable(label))
        {
            return true;
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(hoverLabel);
        }

        return false;
    }

    private static unsafe void SetHovered(string id, bool hovered)
    {
        var storage = ImGuiNative.GetStateStorage();
        var key = ImGui.GetID(id);
        ImGuiNative.SetBool(storage, key, Convert.ToByte(hovered));
    }

    public static unsafe bool Hovered(string id)
    {
        var storage = ImGuiNative.GetStateStorage();
        var key = ImGui.GetID(id);
        return Convert.ToBoolean(ImGuiNative.GetBool(storage, key, Convert.ToByte(false)));
    }
    
    /// <summary>
    /// Using this class will wrap any item in this context with a disabled ImGui.Selectable node which will display if the user hovers it.
    /// This will split the current ImGui DrawList into 2 channels and default to the front one.
    /// </summary>
    public class Hoverable : IDisposable
    {
        public Vector2 StartPos { get; private set; }
        public Vector2 EndPos { get; private set; }
        public Vector2 Margin { get; init; }
        public Vector2 Padding { get; init; }
        public float Rounding { get; init; }
        public bool Highlight { get; init; }
        
        public string Id { get; private set; }

        private ImDrawListPtr draw;

        public Hoverable(string id)
        {
            Id = id;
            Margin = Vector2.Zero;
            Padding = new Vector2(5f, 2f);
            Rounding = 5f;
            Highlight = false;

            Begin();
        }
        
        public Hoverable(string id, float rounding = 5f, Vector2 margin = default(Vector2), Vector2 padding = default(Vector2), bool highlight = false)
        {
            Id = id;
            Margin = margin;
            Padding = padding;
            Rounding = rounding;
            Highlight = highlight;
            
            Begin();
        }

        private void Begin()
        {
            StartPos = ImGui.GetCursorScreenPos();
            
            draw = ImGui.GetWindowDrawList();

            ImGui.ChannelsSplit(draw, 2);
            draw.ChannelsSetCurrent(1);
            
            ImGui.BeginGroup();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + Padding.Y);
            ImGui.Indent(Padding.X);
        }

        public void Dispose()
        {
            ImGui.Unindent();
            ImGui.EndGroup();
            
            EndPos = ImGui.GetCursorScreenPos();
            ImGui.SetCursorScreenPos(StartPos);
            
            draw.ChannelsSetCurrent(0); // Set the channel to the background

            using (ImRaii.Disabled())
            {
                ImGui.Selectable($"###{Id}", false, ImGuiSelectableFlags.None, EndPos - StartPos);
                //ImGui.Button($"###{Id}", EndPos - StartPos);
            }

            var min = StartPos + Margin with { Y = Margin.X };
            var max = EndPos + Margin with { X = Margin.Y + ImGui.GetContentRegionAvail().X };
            var color = ImGui.GetColorU32(ImGuiCol.FrameBg, 0.25f);
            var color1 = ImGui.GetColorU32(ImGuiCol.FrameBg, 0f); // Used for gradient
            var lineColor = ImGui.GetColorU32(ImGuiCol.Tab);
            
            if (ImGui.IsMouseHoveringRect(min, max) && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                color = ImGui.GetColorU32(ImGuiCol.FrameBgHovered);
                color1 = ImGui.GetColorU32(ImGuiCol.FrameBgHovered, 0f);
                lineColor = ImGui.GetColorU32(ImGuiCol.TabActive);
                SetHovered(Id, true);
            }
            else
            {
                SetHovered(Id, false);
            }
            
            if (Highlight)
            {
                draw.AddRectFilledMultiColor(min, max, color, color1, color1, color);
                draw.AddLine(min, EndPos, lineColor);
            }
            else
            {
                draw.AddRectFilled(min, max, color, Rounding, ImDrawFlags.None);
            }
            
            ImGui.SetCursorScreenPos(EndPos);

            draw.ChannelsSetCurrent(1);
            ImGui.ChannelsMerge(draw);
            GC.SuppressFinalize(this);
        }
    }
}
