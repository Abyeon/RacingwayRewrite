using System;
using System.Collections.Generic;
using MessagePack;
using RacingwayRewrite.Race.Collision.Shapes;

namespace RacingwayRewrite.Race.Collision;

[MessagePackObject]
public class Checkpoint : ITrigger
{
    [Key(0)] public Shape Shape { get; set; } 
    [Key(1)] public Behavior TriggerFlags { get; set; } = Behavior.OnlyGrounded;
    [Key(2)] public uint Position { get; set; } = 1;
    [IgnoreMember] public uint Color { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint DefaultColor { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint TriggeredColor { get; set; } = 0x55FF0000;
    [IgnoreMember] public List<uint> Touchers { get; } = [];

    public Checkpoint(Shape shape)
    {
        Shape = shape;
    }
    
    public Checkpoint(Shape shape, Behavior triggerFlags, uint position)
    {
        Shape = shape;
        TriggerFlags = triggerFlags;
        Position = position;
    }

    public void OnEnter(Player player)
    {
        Plugin.Log.Debug("Checkpoint entered");
    }

    public void OnExit(Player player)
    {
        Plugin.Log.Debug("Checkpoint exited");
    }
}
