using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Race;

[MessagePackObject]
public class Route
{
    [Key(0)] public string Name { get; set; }
    [Key(1)] public string Description { get; set; } = "";
    [Key(2)] public Address Address { get; init; }
    [Key(3)] public bool AllowMounts  { get; set; } = false;
    
    [Key(4)] public List<ITrigger> Triggers { get; set; } = [];
    
    // Would be best to have a db for all records and fetch related ones whenever we want them.
    //public List<Record> Records { get; set; } = [];
    
    public Route(string name, Address address)
    {
        Name = name;
        Address = address;
    }

    public Route(string name, string description, Address address, bool allowMounts)
    {
        Name = name;
        Description = description;
        Address = address;
        AllowMounts = allowMounts;
    }

    public void CheckCollision(Player player, bool jumped)
    {
        if (!AllowMounts && player.Mounted)
        {
            player.State.Fail();
        }

        Parallel.ForEach(Triggers,trigger =>
        {
            trigger.CheckCollision(player);
        });
    }

    public void Kick(Player player)
    {
        foreach (var trigger in Triggers)
        {
            trigger.Exit(player);
        }
    }
}
