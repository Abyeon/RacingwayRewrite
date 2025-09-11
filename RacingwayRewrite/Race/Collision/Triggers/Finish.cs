using System;
using System.Collections.Generic;
using MessagePack;
using RacingwayRewrite.Race.Collision.Shapes;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Race.Collision;

[MessagePackObject]
public class Finish : ITrigger
{
    [Key(0)] public Shape Shape { get; set; } 
    [Key(1)] public Behavior TriggerFlags { get; set; } = Behavior.OnlyGrounded;
    [IgnoreMember] public uint Color { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint DefaultColor { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint TriggeredColor { get; set; } = 0x55FF0000;
    [IgnoreMember] public List<uint> Touchers { get; } = [];
    
    public Finish(Shape shape)
    {
        Shape = shape;
    }
    
    public Finish(Shape shape, Behavior triggerFlags)
    {
        Shape = shape;
        TriggerFlags = triggerFlags;
    }
    
    public void OnEnter(Player player)
    {
        player.State.Finish();
        Plugin.Log.Debug("Finish entered");
    }

    public void OnExit(Player player)
    {
        Plugin.Log.Debug("Finish exited");
    }
}

