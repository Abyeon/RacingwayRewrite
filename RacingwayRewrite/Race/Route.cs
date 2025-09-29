using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using MessagePack;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Race.Territory;
using ZLinq;

namespace RacingwayRewrite.Race;

[MessagePackObject]
public class Route
{
    [IgnoreMember] public ObjectId? Id { get; set; }
    [Key(0)] public string Name { get; set; }
    [Key(1)] public string Author { get; set; }
    [Key(2)] public string Description { get; set; } = "";
    [Key(3)] public Address Address { get; init; }
    [Key(4)] public int Laps { get; set; } = 1;
    [Key(5)] public bool AllowMounts  { get; set; } = false;
    [Key(6)] public List<ITrigger> Triggers { get; set; } = [];
    
    // Would be best to have a db for all records and fetch related ones whenever we want them.
    //public List<Record> Records { get; set; } = [];
    
    public Route(string name, string author, Address address)
    {
        Name = name;
        Author = author;
        Address = address;
    }

    public Route(string name, string author, string description, Address address, int laps, bool allowMounts)
    {
        Name = name;
        Author = author;
        Description = description;
        Address = address;
        Laps = laps;
        AllowMounts = allowMounts;
    }

    /// <summary>
    /// Checks the collision between the player and every trigger in this route.
    /// </summary>
    /// <param name="player">The player to check</param>
    public void CheckCollision(Player player)
    {
        // Return early if we don't even have a complete route
        bool traditionalTriggers = Triggers.Exists(x => x is Start) || Triggers.Exists(x => x is Finish);
        if (!traditionalTriggers || Triggers.Exists(x => x is Loop))
            return;
        
        if (player.State.CurrentRoute == this && !AllowMounts && player.Mounted)
        {
            player.State.Fail("Cannot use mounts in this route.");
            Kick(player);
        }

        List<ITrigger> triggersToCheck = [];

        if (player.State.InRace && player.State.CurrentRoute == this)
        {
            triggersToCheck = Triggers;
        }
        else
        {
            // Only check start trigger if not racing in this route
            triggersToCheck.Add(Triggers.Find(x => x is Start)!);
        }

        Parallel.ForEach(triggersToCheck,trigger =>
        {
            trigger.CheckCollision(player);
        });
    }

    /// <summary>
    /// Remove the player from every trigger in this route
    /// </summary>
    /// <param name="player">Player to remove</param>
    public void Kick(Player player)
    {
        foreach (var trigger in Triggers)
        {
            // Weird bug where trigger colors don't get updated when player fails/finishes
            // Best to look at this with awake eyes lol.
            if (trigger.Touchers.Contains(player.Id))
                trigger.Exit(player);

            trigger.UpdateColor();
        }
    }

    /// <summary>
    /// Checks if this route's checkpoint layout is valid
    /// </summary>
    /// <returns>True if the checkpoints are valid, False if not</returns>
    public bool ValidCheckpoints()
    {
        // Get checkpoints in Triggers and then sort by their position
        var checkpoints = Triggers.AsValueEnumerable().Where(x => x is Checkpoint).Cast<Checkpoint>().ToArray();
        Array.Sort(checkpoints, (a, b) => a.Position.CompareTo(b.Position));
        
        if (checkpoints.Length <= 1)
        {
            switch (checkpoints.Length)
            {
                case 0:
                case 1 when checkpoints[0].Position == 1:
                    return true;
                default:
                    return false;
            }
        }
        
        // Check if there are any gaps between checkpoints
        for (var i = 1; i < checkpoints.Length; i++)
        {
            var lastPos = checkpoints[i-1].Position;
            var pos = checkpoints[i].Position;

            if (lastPos < 1) throw new ConstraintException("Checkpoint positions cannot be less than 1.");

            if (pos - lastPos > 1) return false;
        }
        
        return true;
    }

    [IgnoreMember]
    public uint LastCheckpoint
    {
        get
        {
            var checkpoints = Triggers.AsValueEnumerable().Where(x => x is Checkpoint).Cast<Checkpoint>().ToArray();
            return checkpoints.Length == 0 ? 0 : checkpoints.Max(x => x.Position);
        }
    }
}
