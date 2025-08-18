using System;
using System.Collections.Generic;
using RacingwayRewrite.Race.Collision;

namespace RacingwayRewrite.Race;

public class Route
{
    public string Name { get; set; }
    public string Description { get; set; } = "";
    
    public List<ITrigger> Triggers { get; set; } = [];
    public List<Record> Records { get; set; } = [];
    
    public Route(string name)
    {
        Name = name;
    }

    public void CheckCollision(Player player)
    {
        // TODO: Add behaviour settings to routes, and move this to there
        if (player.Mounted)
        {
            Kick(player);
        }
        
        foreach (var trigger in Triggers)
        {
            trigger.CheckCollision(player);
        }
    }

    public void Kick(Player player)
    {
        foreach (var trigger in Triggers)
        {
            trigger.Exit(player);
        }
    }
}
