using System;
using System.Diagnostics;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Race;

/// <summary>
/// Describes the state the player is in during a race
/// </summary>
public class RaceState(Player player)
{
    public Route? CurrentRoute { get; set; }
    public bool InRace { get; set; }
    public uint Checkpoint { get; set; }
    public uint Lap { get; set; }
    public Stopwatch Timer { get; set; } = new();

    /// <summary>
    /// Start's the timer for the player
    /// </summary>
    /// <param name="route">The route the player is in</param>
    public void Start(Route route)
    {
        if (InRace && route != CurrentRoute)
        {
            Plugin.Chat.Warning("Tried to start a race in a route while actively in a race!" +
                                " If this was intended, quit your last race with /race quit");
            //Plugin.Chat.Warning("");
            return;
        }

        if (!route.ValidCheckpoints())
        {
            Plugin.Chat.Warning("This route's checkpoints are not set up correctly!\n" +
                                "Please check that each checkpoint position increments by one or matches that of another checkpoint (this allows for branching paths.)");
            return;
        }

        Checkpoint = 0;
        Lap = 0;
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
        if (!InRace) return;
        
        // Player already hit this checkpoint
        if (position <= Checkpoint) return;
        
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
    /// Silently kicks the player out of their current race.
    /// </summary>
    public void SilentFail()
    {
        if (!InRace) return;
        if (CurrentRoute == null) throw new NullReferenceException("Route was null");
        
        Timer.Reset();
        Checkpoint = 0;
        InRace = false;
        CurrentRoute?.Kick(player);
        CurrentRoute = null;
    }
    
    /// <summary>
    /// Kicks the player out of their current race
    /// </summary>
    public void Fail(string reason = "")
    {
        if (!InRace) return;
        
        SilentFail();

        if (player.IsClient && Plugin.Configuration.AllowChat)
        {
            Plugin.Chat.Warning($"Failed race! {reason}");
        }
    }

    /// <summary>
    /// Handle logic for incrementing lap count in races with loops
    /// </summary>
    public void Loop()
    {
        if (CurrentRoute == null)
        {
            Plugin.Chat.Error("The current route is null! This should never happen.");
            return;
        }
        
        if (Checkpoint != CurrentRoute.LastCheckpoint)
        {
            Plugin.Chat.Warning("You have not hit every checkpoint in this route yet! Turn back!");
            return;
        }
        
        Lap++;
        Checkpoint = 0;
        
        // Reached needed lap count
        if (Lap == CurrentRoute.Laps)
        {
            Finish();
        }
    }

    /// <summary>
    /// Handle logic for player finishing a race
    /// </summary>
    public void Finish()
    {
        if (CurrentRoute == null)
        {
            Plugin.Chat.Error("The current route is null! This should never happen.");
            return;
        }
        
        if (Checkpoint != CurrentRoute.LastCheckpoint)
        {
            Plugin.Chat.Print(Checkpoint.ToString() +" "+CurrentRoute.LastCheckpoint.ToString());
            Plugin.Chat.Warning("You have not hit every checkpoint in this route yet! Turn back!");
            return;
        }
        
        TimeSpan elapsed = Timer.Elapsed;
        Timer.Stop();
        
        var guid = Guid.NewGuid();
        Plugin.VfxManager.AddVfx(new Vfx(guid.ToString(), "itm_tape_01c", player.Character), 3000);
        
        Checkpoint = 0;
        Lap = 0;
        
        CurrentRoute?.Kick(player);
        CurrentRoute = null;
        InRace = false;
        
        if (Plugin.Configuration.AllowChat)
        {
            Plugin.Chat.PrintPlayer(player, $"finished race in {Time.PrettyFormatTimeSpan(elapsed)}", true);
        }
    }
}
