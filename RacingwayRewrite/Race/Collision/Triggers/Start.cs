using System;
using System.Collections.Generic;
using MessagePack;
using RacingwayRewrite.Race.Collision.Shapes;

namespace RacingwayRewrite.Race.Collision.Triggers;

[MessagePackObject]
public class Start : ITrigger
{
    [Key(0)] public Shape Shape { get; set; } 
    [Key(1)] public Behavior TriggerFlags { get; set; } = Behavior.OnlyGrounded;
    [IgnoreMember] public uint Color { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint DefaultColor { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint TriggeredColor { get; set; } = 0x55FF0000;
    [IgnoreMember] public List<uint> Touchers { get; } = [];

    [IgnoreMember] public Route? Route { get; set; } = null;

    public Start(Shape shape)
    {
        Shape = shape;
    }
    
    public Start(Shape shape, Behavior triggerFlags)
    {
        Shape = shape;
        TriggerFlags = triggerFlags;
    }

    public void OnEnter(Player player)
    {
        Plugin.Log.Verbose("Start entered");
        player.State.SilentFail();
    }

    public void OnExit(Player player)
    {
        if (Route == null) throw new NullReferenceException("Route is null");
        
        player.State.Start(Route);
        Plugin.Log.Debug("Start exited");
    }
}
