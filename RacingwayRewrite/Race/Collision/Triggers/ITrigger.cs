using System;
using System.Collections.Generic;
using MessagePack;
using RacingwayRewrite.Race.Collision.Shapes;

namespace RacingwayRewrite.Race.Collision;

public enum Behavior : ushort
{
    Always = 0,
    OnlyGrounded = 1,
    OnlyJumping = 2
}

[Union(0, typeof(Start))]
[Union(1, typeof(Checkpoint))]
[Union(2, typeof(Fail))]
[Union(3, typeof(Finish))]
public interface ITrigger
{
    Shape Shape { get; set; }

    uint Color { get; set; }
    uint DefaultColor { get; set; }
    uint TriggeredColor { get; set; }

    Behavior TriggerFlags { get; set; }
    List<uint> Touchers { get; }

    void CheckCollision(Player player)
    {
        bool collides = Shape.PointInside(player.Position);

        // Player left the trigger
        if (Touchers.Contains(player.Id) && !collides)
        {
            Exit(player);
            return;
        }

        // Stop checking logic if the player doesn't collide
        if (!collides) return;
        
        // Player enters the trigger
        if (!Touchers.Contains(player.Id))
        {
            switch (TriggerFlags)
            {
                case Behavior.Always:
                    Enter(player);
                    return;
                case Behavior.OnlyGrounded:
                    if (player.Grounded) Enter(player);
                    return;
                case Behavior.OnlyJumping:
                    if (!player.Grounded) Enter(player);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(TriggerFlags), "Invalid behavior");
            }
        }
        
        // Player is still in trigger
        switch (TriggerFlags)
        {
            case Behavior.Always:
                return;
            case Behavior.OnlyGrounded:
                if (!player.Grounded) Exit(player);
                return;
            case Behavior.OnlyJumping:
                if (player.Grounded) Exit(player);
                return;
        }
    }

    private void UpdateColor()
    {
        Color = Touchers.Count == 0 ? DefaultColor : TriggeredColor;
    }

    private void Enter(Player player)
    {
        OnEnter(player);
        Touchers.Add(player.Id);
        UpdateColor();
    }

    void Exit(Player player)
    {
        OnExit(player);
        Touchers.Remove(player.Id);
        UpdateColor();
    }

    void OnEnter(Player player);
    void OnExit(Player player);
}
