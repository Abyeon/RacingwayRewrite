using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
namespace RacingwayRewrite.Utils;

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
    /// <returns>True if the input was updated</returns>
    public static bool AddTextConfirmationPopup(string id, string description, ref string input, int maxLength = 512)
    {
        using var popup = ImRaii.Popup(id);
        if (popup.Success)
        {
            ImGui.PushID(id);
            ImGui.Text(description);
            ImGui.Separator();

            ImGui.InputText("##textInput", ref Buf, maxLength);

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
    
    /// <summary>
    /// Using this class will wrap any item in this context with a disabled ImGui.Selectable node which will display if the user hovers it.
    /// This will split the current ImGui DrawList into 2 channels and default to the front one.
    /// </summary>
    public class Hoverable : IDisposable
    {
        public Vector2 StartPos { get; init; }
        public Vector2 EndPos { get; private set; }
        
        public bool Disposed { get; private set; }
        public string Id { get; private set; }

        private ImDrawListPtr draw;

        public Hoverable(string id)
        {
            StartPos = ImGui.GetCursorScreenPos();
            
            Id = id;
            draw = ImGui.GetWindowDrawList();

            ImGui.ChannelsSplit(draw, 2);
            draw.ChannelsSetCurrent(1);
        }

        public void Dispose()
        {
            EndPos = ImGui.GetCursorScreenPos();
            ImGui.SetCursorScreenPos(StartPos);
            draw.ChannelsSetCurrent(0); // Set the channel to the background
            
            using (ImRaii.Disabled())
                ImGui.Selectable($"##{Id}", false, ImGuiSelectableFlags.AllowItemOverlap, EndPos - StartPos);

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                var min = StartPos + new Vector2(-2f, -2f);
                var max = EndPos + new Vector2(2f + ImGui.GetContentRegionAvail().X, 2f);
                var color = ImGui.GetColorU32(ImGuiCol.FrameBgHovered);
                
                draw.AddRectFilled(min, max, color, 5f, ImDrawFlags.None);
            }
            
            ImGui.SetCursorScreenPos(EndPos);

            draw.ChannelsSetCurrent(1);
            ImGui.ChannelsMerge(draw);
            
            Disposed = true;
        }
    }
}
