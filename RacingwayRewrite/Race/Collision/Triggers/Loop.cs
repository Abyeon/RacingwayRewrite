using System.Collections.Generic;
using MessagePack;
using RacingwayRewrite.Race.Collision.Shapes;

namespace RacingwayRewrite.Race.Collision.Triggers;

[MessagePackObject]
public class Loop : ITrigger
{
    [Key(0)] public Shape Shape { get; set; } 
    [Key(1)] public Behavior TriggerFlags { get; set; } = Behavior.OnlyGrounded;
    [IgnoreMember] public uint Color { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint DefaultColor { get; set; } = 0x5500FF0F;
    [IgnoreMember] public uint TriggeredColor { get; set; } = 0x55FF0000;
    [IgnoreMember] public List<uint> Touchers { get; } = [];
    
    [IgnoreMember] public Route? Route { get; set; } = null;
    

    public Loop(Shape shape)
    {
        Shape = shape;
    }
    
    public Loop(Shape shape, Behavior triggerFlags)
    {
        Shape = shape;
        TriggerFlags = triggerFlags;
    }

    public void OnEnter(Player player)
    {
        if (player.State.InRace)
            player.State.Loop();
        
        Plugin.Log.Verbose("Loop entered");
    }

    public void OnExit(Player player)
    {
        if (!player.State.InRace && Route != null)
            player.State.Start(Route);
        
        Plugin.Log.Debug("Loop exited");
    }
}
