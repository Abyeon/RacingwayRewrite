using System;
using System.Diagnostics;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Race;

/// <summary>
/// Describes the state the player is in during a race
/// </summary>
public class RaceState(Player player)
{
    public Route? CurrentRoute { get; set; }
    public bool InRace { get; set; } = false;
    public uint Checkpoint { get; set; } = 0;
    public uint Lap { get; set; } = 0;
    public Stopwatch Timer { get; set; } = new Stopwatch();

    /// <summary>
    /// Start's the timer for the player
    /// </summary>
    /// <param name="route">The route the player is in</param>
    public void Start(Route route)
    {
        CurrentRoute = route;
        Timer.Restart();
        InRace = true;
    }
    
    /// <summary>
    /// Increments the player's checkpoint to the hit one, if they have hit the previous points.
    /// </summary>
    /// <param name="position"></param>
    public void HitCheckpoint(uint position)
    {
        if (Checkpoint + 1 == position)
        {
            Checkpoint = position;
        }
        else if (player.IsClient && Plugin.Configuration.AllowChat)
        {
            Plugin.Chat.Warning("You've missed a checkpoint somewhere! Try going back.");
            Plugin.Log.Verbose($"{player.Name} tried hitting a checkpoint without going through the previous ones.");
        }
    }
    
    /// <summary>
    /// Kicks the player out of their current race
    /// </summary>
    public void Fail()
    {
        if (CurrentRoute == null) throw new NullReferenceException("Route was null");
        
        Timer.Reset();
        Checkpoint = 0;
        InRace = false;
        CurrentRoute.Kick(player);

        if (player.IsClient && Plugin.Configuration.AllowChat)
        {
            Plugin.Chat.Warning("Failed race!");
        }
    }

    /// <summary>
    /// Handle logic for incrementing lap count in races with loops
    /// </summary>
    public void Loop()
    {
        // TODO: Implement lap count in Route
    }

    /// <summary>
    /// Handle logic for player finishing a race
    /// </summary>
    public void Finish()
    {
        TimeSpan elapsed = Timer.Elapsed;
        Timer.Stop();
        
        if (Plugin.Configuration.AllowChat)
        {
            Plugin.Chat.PrintPlayer(player, $"finished race in {Time.PrettyFormatTimeSpan(elapsed)}");
        }
    }
}
