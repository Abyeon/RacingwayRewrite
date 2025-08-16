using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Race.Collision;

namespace RacingwayRewrite.Race;

public class RaceManager : IDisposable
{
    internal readonly Plugin Plugin;
    internal readonly IFramework Framework;
    internal readonly IObjectTable ObjectTable;
    internal readonly IClientState ClientState;
    
    public RaceManager(Plugin plugin, IFramework framework, IObjectTable objectTable, IClientState clientState)
    {
        this.Plugin = plugin;
        this.Framework = framework;
        this.ObjectTable = objectTable;
        this.ClientState = clientState;
        
        Framework.Update += Update;
    }

    private IBattleChara? localPlayer;
    
    private void Update(IFramework framework)
    {
        localPlayer = ClientState.LocalPlayer;
        if (localPlayer == null) return; // Return if the player is null
        if (ClientState.IsPvPExcludingDen) return; // Return if the player is in pvp

        TrackPlayer(localPlayer);

        if (Plugin.Configuration.TrackOthers)
        {
            IEnumerable<IBattleChara> playersToTrack = ObjectTable.PlayerObjects.Where(x => x.EntityId != localPlayer.EntityId);
            List<uint> touchedIds = new List<uint>();
            
            foreach (var player in ObjectTable.PlayerObjects)
            {
                TrackPlayer(player);
                touchedIds.Add(player.EntityId);
            }
            
            // Cleanup old/lost players
            if (playersToTrack.Count() + 1 != Players.Count)
            {
                List<uint> keysToRemove = Players.Keys.Where(key => !touchedIds.Contains(key)).ToList();
                foreach (var key in keysToRemove)
                {
                    if (key == localPlayer.EntityId) continue;
                    Players.Remove(key);
                }
            }
        } else if (Players.Count > 1)
        {
            // Remove tracked players after client disabled "track others"
            foreach (var player in Players.Keys.Where(key => key != localPlayer.EntityId))
            {
                Players.Remove(player);
            }
        }
    }

    public readonly Dictionary<uint, Player> Players = new Dictionary<uint, Player>();
    public List<Cube> Cubes = new List<Cube>();
    
    private void TrackPlayer(IBattleChara actor)
    {
        if (!Players.ContainsKey(actor.EntityId))
        {
            Player player = new Player(actor);
            Players.Add(actor.EntityId, player);
            Plugin.Log.Debug(Players.Count.ToString());
            
            bool lastGrounded = player.Grounded;
            bool lastMounted = player.Mounted;
            
            player.UpdateState(actor);
            PlayerUpdated(player, actor, lastGrounded, lastMounted);
        }
        else
        {
            Player player = Players[actor.EntityId];
            
            bool lastGrounded = player.Grounded;
            bool lastMounted = player.Mounted;
            
            player.UpdateState(actor);
            if (player.Position != actor.Position || lastGrounded != player.Grounded || lastMounted != player.Mounted)
            {
                PlayerUpdated(player, actor, lastGrounded, lastMounted);
            }
        }
    }

    private void PlayerUpdated(Player player, IBattleChara actor, bool lastGrounded, bool lastMounted)
    {
        Vector3 lastPos = player.Position;
        
        // Moved
        if (lastPos != actor.Position)
        {
            player.Position = actor.Position;
        }

        foreach (Cube cube in Cubes)
        {
            if (cube.PointInside(player.Position))
            {
                Plugin.Log.Debug($"{player.Name} is inside of cube!");
            }
        }
    }

    private void CleanupPlayer(Player player)
    {
        // TODO: Remove player from all routes/triggers
    }

    public void Dispose()
    {
        this.Framework.Update -= Update;

        foreach (var player in Players.Values)
        {
            CleanupPlayer(player);
        }
        
        Players.Clear();
        Cubes.Clear();
        
        localPlayer = null;
    }
}
