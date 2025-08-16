using System;
using System.Collections.Generic;

namespace RacingwayRewrite.Race.Collision;

[Flags]
public enum Behavior : ushort
{
    Always = 0,
    OnlyGrounded = 1,
    OnlyJumping = 2,
    AllowMounts = 3
}

public interface ITrigger
{
    public Shape Shape { get; set; }
    
    public uint Color { get; set; }
    public uint DefaultColor { get; set; }
    public uint TriggeredColor { get; set; }
    
    public Behavior TriggerFlags { get; set; }
    
    public List<uint> Touchers { get; set; }

    public void CheckCollision(Player player)
    {
        bool collides = Shape.PointInside(player.Position);
        if (player.Mounted && !TriggerFlags.HasFlag(Behavior.AllowMounts)) return;

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
            if (TriggerFlags.HasFlag(Behavior.Always))
            {
                Enter(player);
                return;
            }

            if (TriggerFlags.HasFlag(Behavior.OnlyGrounded) && player.Grounded)
            {
                Enter(player);
                return;
            }

            if (TriggerFlags.HasFlag(Behavior.OnlyJumping) && !player.Grounded)
            {
                Enter(player);
            }
        }
        else
        {
            if (TriggerFlags.HasFlag(Behavior.Always))
            {
                Exit(player);
                return;
            }
            
            if (TriggerFlags.HasFlag(Behavior.OnlyGrounded) && !player.Grounded)
            {
                Exit(player);
                return;
            }

            if (TriggerFlags.HasFlag(Behavior.OnlyJumping) && player.Grounded)
            {
                Exit(player);
            }
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
