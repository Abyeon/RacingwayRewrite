using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Race;

public class Route
{
    public string Name { get; set; }
    public string Description { get; set; } = "";
    public Address Address { get; init; }
    public bool AllowMounts  { get; set; } = false;
    
    public List<ITrigger> Triggers { get; set; } = [];
    
    // Would be best to have a db for all records and fetch related ones whenever we want them.
    //public List<Record> Records { get; set; } = [];
    
    public Route(string name, Address address)
    {
        Name = name;
        Address = address;
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
