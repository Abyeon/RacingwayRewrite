using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.Sheets;
using MessagePack;
using RacingwayRewrite.Race.Collision.Shapes;
using RacingwayRewrite.Utils.Interop;

namespace RacingwayRewrite.Race.Collision.Triggers;

[MessagePackObject]
public class Fail : ITrigger
{
    [Key(0)] public Shape Shape { get; set; } 
    [Key(1)] public Behavior TriggerFlags { get; set; } = Behavior.Always;
    [IgnoreMember] public uint Color { get; set; } = 0x551111FF;
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
        Plugin.VfxManager.AddVfx(new ActorVfx("vfx/monster/gimmick2/eff/e3d2_b2_g05t0x.avfx", player.Character!, player.Character!, TimeSpan.FromSeconds(2)));

        player.State.Fail();
        
        Plugin.Log.Debug("Fail entered");
    }

    public void OnExit(Player player)
    {
        Plugin.Log.Debug("Fail exited");
    }
}
