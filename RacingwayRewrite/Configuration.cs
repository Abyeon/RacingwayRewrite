using Dalamud.Configuration;
using System;
using System.Numerics;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.GameFonts;

namespace RacingwayRewrite;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    // --- Main Settings ---
    public bool TrackOthers { get; set; } = false;
    public bool ShowOverlay { get; set; } = false;
    public bool AllowChat { get; set; } = true;

    // --- Font Settings ---
    public SingleFontSpec TimerFont { get; set; } = new()
    {
        FontId = new GameFontAndFamilyId(GameFontFamily.Axis),
        SizePt = 34.0f
    };

    public Vector4? TimerColor { get; set; } = null;
    public Vector4? TimerBackgroundColor { get; set; } = null;
    public float TimerRounding { get; set; }= 0f;
    
    // --- DEBUG SETTINGS ---    
    public bool DebugMode { get; set; } = false;
    public bool OpenWindowsOnStartup { get; set; } = false;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
