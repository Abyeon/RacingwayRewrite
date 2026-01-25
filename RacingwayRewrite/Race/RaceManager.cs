using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Utils;
using RacingwayRewrite.Utils.Structs;
using ZLinq;

namespace RacingwayRewrite.Race;

public class RaceManager : IDisposable
{
    public RouteLoader RouteLoader { get; private set; }
    
    internal readonly IFramework Framework;
    internal readonly IObjectTable ObjectTable;
    internal readonly IClientState ClientState;
    internal readonly ActorManager ActorManager;
    internal TimelineHook TimelineHook;
    
    public RaceManager(Plugin plugin, IFramework framework, IObjectTable objectTable, IClientState clientState)
    {
        Framework = framework;
        ObjectTable = objectTable;
        ClientState = clientState;
        ActorManager = new ActorManager(clientState);
        TimelineHook = new TimelineHook();
        
        Framework.Update += Update;
        RouteLoader = new RouteLoader(plugin, clientState);
    }

    private IBattleChara? localPlayer;
    
    private void Update(IFramework framework)
    {
        localPlayer = ObjectTable.LocalPlayer;
        if (localPlayer == null) return; // Return if the player is null
        if (ClientState.IsPvPExcludingDen) return; // Return if the player is in pvp
        
        if (Plugin.Configuration.TrackOthers)
        {
            List<uint> touchedIds = new List<uint>();
            
            foreach (var player in ObjectTable.PlayerObjects)
            {
                TrackPlayer(player);
                touchedIds.Add(player.EntityId);
            }
            
            // Cleanup old/lost players
            Task.Run(() =>
            {
                var keysToRemove = Players.Keys.AsValueEnumerable().Where(key => !touchedIds.Contains(key)).ToList();
                foreach (var key in keysToRemove)
                {
                    if (key == localPlayer.EntityId) continue;
                    Players.Remove(key);
                }
            });
        }
        else
        {
            TrackPlayer(localPlayer);
            
            // Remove tracked players after client disabled "track others"
            if (Players.Count > 1)
            {
                foreach (var player in Players.Keys.Where(key => key != localPlayer.EntityId))
                {
                    Players.Remove(player);
                }
            }
        }
    }

    public readonly Dictionary<uint, Player> Players = new();
    public int SelectedTrigger = -1;

    public Player? GetPlayer(uint entityId)
    {
        return Players.GetValueOrDefault(entityId);
    }
    
    private void TrackPlayer(IBattleChara? actor)
    {
        try
        {
            if (actor == null) return;

            if (!Players.TryGetValue(actor.EntityId, out var player))
            {
                Player newPlayer = new Player(actor);
                Players.Add(actor.EntityId, newPlayer);

                newPlayer.UpdateState(actor, (float)Framework.UpdateDelta.TotalSeconds);
                PlayerUpdated(newPlayer, actor);
            }
            else
            {
                player.Rotation = actor.Rotation;

                bool lastGrounded = player.Grounded;
                bool lastMounted = player.Mounted;

                player.UpdateState(actor, (float)Framework.UpdateDelta.TotalSeconds);
                if (player.Position != actor.Position || lastGrounded != player.Grounded ||
                    lastMounted != player.Mounted)
                {
                    PlayerUpdated(player, actor);
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e.ToString());
        }
    }
    
    private void PlayerUpdated(Player player, IBattleChara actor)
    {
        player.LastPosition = player.Position;
        
        // Moved
        if (player.LastPosition != actor.Position)
        {
            // Player jumped and their state was not changed.
            if (player is { Grounded: false, LastVelocity.Y: < 0, Velocity.Y: > 0 })
            {
                player.Grounded = true;
            }
            
            player.Position = actor.Position;
        }

        foreach (var route in RouteLoader.LoadedRoutes)
        {
            route.CheckCollision(player);
        }
    }

    /// <summary>
    /// Removes all players from their races
    /// </summary>
    public void KickAllPlayers()
    {
        foreach (var player in Players.Values.ToList())
        {
            player.State.SilentFail();
        }
    }

    public void Dispose()
    {
        Framework.Update -= Update;
        
        ActorManager.Dispose();
        TimelineHook.Dispose();
        
        Players.Clear();
        
        RouteLoader.Dispose();
        
        localPlayer = null;
    }
}
