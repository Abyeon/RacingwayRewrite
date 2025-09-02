using System;
using System.Collections.Generic;
using RacingwayRewrite.Race.Collision.Shapes;

namespace RacingwayRewrite.Race.Collision;

public class Fail(Shape shape) : ITrigger
{
    public Shape Shape { get; set; } = shape;
    public uint Color { get; set; } = 0x5500FF0F;
    public uint DefaultColor { get; set; } = 0x551111FF;
    public uint TriggeredColor { get; set; } = 0x55FF0000;
    public Behavior TriggerFlags { get; set; } = Behavior.Always;
    public List<uint> Touchers { get; } = [];

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
