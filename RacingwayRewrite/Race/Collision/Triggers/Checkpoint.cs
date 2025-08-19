using System.Collections.Generic;
using RacingwayRewrite.Race.Collision.Shapes;

namespace RacingwayRewrite.Race.Collision;

public class Checkpoint(Shape shape) : ITrigger
{
    public Shape Shape { get; set; } = shape;
    public uint Color { get; set; } = 0x5500FF0F;
    public uint DefaultColor { get; set; } = 0x5500FF0F;
    public uint TriggeredColor { get; set; } = 0x55FF0000;
    public Behavior TriggerFlags { get; set; } = Behavior.OnlyGrounded;
    public List<uint> Touchers { get; } = [];

    public void OnEnter(Player player)
    {
        Plugin.Log.Debug("Checkpoint entered");
    }

    public void OnExit(Player player)
    {
        Plugin.Log.Debug("Checkpoint exited");
    }
}
