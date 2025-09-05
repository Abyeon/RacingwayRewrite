using System;
using System.Collections.Generic;
using MessagePack;
using RacingwayRewrite.Race.Collision.Shapes;

namespace RacingwayRewrite.Race.Collision;

[MessagePackObject]
public class Fail: ITrigger
{
    [Key(0)] public Shape Shape { get; set; } 
    [Key(1)] public Behavior TriggerFlags { get; set; } = Behavior.Always;
    [IgnoreMember] public uint Color { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint DefaultColor { get; set; } = 0x551111FF;
    [IgnoreMember] public uint TriggeredColor { get; set; } = 0x55FF0000;
    [IgnoreMember] public List<uint> Touchers { get; } = [];

    public Fail(Shape shape)
    {
        Shape = shape;
    }
    
    public Fail(Shape shape, Behavior triggerFlags)
    {
        Shape = shape;
        TriggerFlags = triggerFlags;
    }

    public void OnEnter(Player player)
    {
        Guid guid = Guid.NewGuid();
        if (player.LastVelocity.Y < -30)
        {
            Plugin.VfxManager.AddVfx(new Vfx(guid.ToString(), "crgim_dumf0c", player.Character), 1000);
        }
        else
        {
            // X marker over player
            // vfx/monster/gimmick2/eff/e3d2_b2_g05t0x.avfx
            // Check mark over player
            // vfx/monster/gimmick2/eff/e3d2_b2_g04t0x.avfx
            
            Plugin.VfxManager.AddVfx(new Vfx(guid.ToString(), "dk05th_stdn0t", player.Character), 2000);
        }
        
        Plugin.Log.Debug("Fail entered");
    }

    public void OnExit(Player player)
    {
        Plugin.Log.Debug("Fail exited");
    }
}
