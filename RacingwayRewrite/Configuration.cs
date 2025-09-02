using Dalamud.Configuration;
using System;
using System.Numerics;

namespace RacingwayRewrite;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool TrackOthers { get; set; } = false;
    public bool ShowDebug { get; set; } = false;
    public bool AllowChat { get; set; } = true;

    public Vector3 Scale { get; set; } = Vector3.One;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
