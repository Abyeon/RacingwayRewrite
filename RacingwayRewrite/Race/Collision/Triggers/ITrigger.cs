using System;
using System.Collections.Generic;

namespace RacingwayRewrite.Race.Collision;

public enum Behavior : ushort
{
    Always = 0,
    OnlyGrounded = 1,
    OnlyJumping = 2
}

public interface ITrigger
{
    public Shape Shape { get; set; }
    
    public uint Color { get; set; }
    public uint DefaultColor { get; set; }
    public uint TriggeredColor { get; set; }
    
    public Behavior TriggerFlags { get; set; }
    
    public List<uint> Touchers { get; }

    public void CheckCollision(Player player)
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

    public void Exit(Player player)
    {
        OnExit(player);
        Touchers.Remove(player.Id);
        UpdateColor();
    }
    
    public void OnEnter(Player player);
    public void OnExit(Player player);
}
