using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Pictomancy;
using RacingwayRewrite.Utils.Interop;

namespace RacingwayRewrite.Utils;

// Eventually want to move from Pictomancy to something more custom for this. For now works fine though.
/// <summary>
/// Handles spawning/deleting custom VFX from the scene
/// </summary>
public class VfxManager : IDisposable
{
    internal readonly IFramework Framework;
    internal readonly VfxFunctions Functions;
    internal const uint MaxVfx = 60;

    public VfxManager(IFramework framework)
    {
        Framework = framework;
        Framework.Update += Update;
        Functions = new VfxFunctions();
    }

    private Dictionary<Vfx, DateTime> currentVfx = new();

    public void AddVfx(Vfx vfx, int timeoutMs)
    {
        // Remove first vfx if over the max vfx count
        if (currentVfx.Count + 1 > MaxVfx)
        {
            var first = currentVfx.OrderBy(x => x.Value).First();
            currentVfx.Remove(first.Key);
        }
        
        currentVfx.Add(vfx, DateTime.Now.AddMilliseconds(timeoutMs));
    }

    private void Update(IFramework framework)
    {
        try
        {
            foreach (var vfx in currentVfx)
            {
                if (vfx.Value < DateTime.Now || vfx.Key.Target == null)
                {
                    currentVfx.Remove(vfx.Key);
                    continue;
                }

                PictoService.VfxRenderer.AddCommon(vfx.Key.Id, vfx.Key.Name, vfx.Key.Target, vfx.Key.Scale, vfx.Key.Color);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex.ToString());
        }
    }

    public void Dispose()
    {
        currentVfx = [];
        Framework.Update -= Update;
        GC.SuppressFinalize(this);
    }
}

public record struct Vfx(string Id, string Name, IPlayerCharacter? Target, Vector3? Scale = null, Vector4? Color = null)
{
    public readonly string Id = Id, Name = Name;
    public readonly IPlayerCharacter? Target = Target;
    public readonly Vector3? Scale = Scale;
    public readonly Vector4? Color = Color;
}
