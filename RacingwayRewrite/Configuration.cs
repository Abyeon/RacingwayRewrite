using Dalamud.Configuration;
using System;

namespace RacingwayRewrite;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool TrackOthers { get; set; } = false;
    public bool ShowOverlay { get; set; } = false;
    public bool AllowChat { get; set; } = true;
    
    public bool DebugMode { get; set; } = false;
    public bool OpenWindowsOnStartup { get; set; } = false;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
