using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

namespace RacingwayRewrite.Race;

public class RaceManager
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

    private IBattleChara? localPlayer = null;
    
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
            if (playersToTrack.Count() + 1 != players.Count)
            {
                List<uint> keysToRemove = players.Keys.Where(key => !touchedIds.Contains(key)).ToList();
                foreach (var key in keysToRemove)
                {
                    players.Remove(key);
                }
            }
        } else if (players.Count > 1)
        {
            // Remove tracked players after client disabled "track others"
            foreach (var player in players.Keys.Where(key => key != localPlayer.EntityId))
            {
                players.Remove(player);
            }
        }
    }

    private Dictionary<uint, Player> players = new Dictionary<uint, Player>();
    
    private void TrackPlayer(IBattleChara actor)
    {
        if (!players.ContainsKey(actor.EntityId))
        {
            Player player = new Player(actor);
            players.Add(actor.EntityId, player);
            PlayerMoved(player, actor.Position);
        }
        else
        {
            Player player = players[actor.EntityId];
            if (player.Position != actor.Position)
            {
                // Player moved
                PlayerMoved(player, actor.Position);
            }
        }
    }

    private void PlayerMoved(Player player, Vector3 position)
    {
        Vector3 lastPos = player.Position;
        player.Position = position;
        //Plugin.Log.Debug($"{player.Name} moved from {lastPos} to {position}");
    }
}
