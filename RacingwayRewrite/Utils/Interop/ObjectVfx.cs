using System;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Common.Math;
using RacingwayRewrite.Utils.Interop.Structs;

namespace RacingwayRewrite.Utils.Interop;

/// <summary>
/// Rewrite of Picto's GameObjectVFX class
/// </summary>
public unsafe class ObjectVfx : BaseVfx
{
    public IGameObject Target;
    public IGameObject Source;
    
    public ObjectVfx(string path, IGameObject target, IGameObject source, TimeSpan? expiration = null)
    {
        if (Plugin.VfxFunctions == null) throw new NullReferenceException("Vfx functions are not initialized");
        
        Path = path;
        Target = target;
        Source = source;

        if (expiration.HasValue)
        {
            Expires = DateTime.UtcNow + expiration.Value;
        }
        else
        {
            Loop = true;
        }
        
        Data = Plugin.VfxFunctions.CreateGameObjectVfx(path, target.Address, source.Address);
    }

    protected override void Refresh()
    {
        Data = Plugin.VfxFunctions.CreateGameObjectVfx(Path, Target.Address, Source.Address);
    }
}
