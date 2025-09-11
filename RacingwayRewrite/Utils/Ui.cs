using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
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
    
    public class Wrap(string id) : IDisposable
    {
        public Vector2 StartPos { get; init; } = ImGui.GetCursorPos();
        public Vector2 EndPos { get; private set; }
        
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            EndPos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(StartPos);
            
            using (ImRaii.Disabled())
                ImGui.Selectable($"##{id}", false, ImGuiSelectableFlags.AllowItemOverlap, EndPos - StartPos);

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                var draw = ImGui.GetWindowDrawList();
                var windowPos = ImGui.GetWindowPos();
                var min = windowPos + (StartPos + new Vector2(-2f, -2f));
                var max = windowPos + (EndPos + new Vector2(2f + ImGui.GetContentRegionAvail().X, 2f));
            
                draw.AddRect(min, max, 0x55FFFFFF, ImDrawFlags.None, 1f);
            }
            
            ImGui.SetCursorPos(EndPos);
            
            Disposed = true;
        }
    }
}
