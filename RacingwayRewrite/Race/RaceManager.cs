using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Utils;
using ZLinq;

namespace RacingwayRewrite.Race;

public class RaceManager : IDisposable
{
    internal readonly Plugin Plugin;
    internal readonly IFramework Framework;
    internal readonly IObjectTable ObjectTable;
    internal readonly IClientState ClientState;

    internal static TerritoryTools TerritoryTools { get; private set; } = null!;
    
    public RaceManager(Plugin plugin, IFramework framework, IObjectTable objectTable, IClientState clientState)
    {
        Plugin = plugin;
        Framework = framework;
        ObjectTable = objectTable;
        ClientState = clientState;
        
        Framework.Update += Update;
        
        TerritoryTools = new TerritoryTools(clientState);
        //TerritoryChanged(ClientState.TerritoryType);
    }

    private IBattleChara? localPlayer;
    
    private void Update(IFramework framework)
    {
        localPlayer = ClientState.LocalPlayer;
        if (localPlayer == null) return; // Return if the player is null
        if (ClientState.IsPvPExcludingDen) return; // Return if the player is in pvp
        
        // Experiments with showing confetti.
        // TODO: Implement a VFX handler for spawning/stopping vfx
        //PictoService.VfxRenderer.AddCommon($"{localPlayer.EntityId}", "itm_tape_01c", localPlayer);
        
        if (Plugin.Configuration.TrackOthers)
        {
            IEnumerable<IBattleChara> playersToTrack = ObjectTable.PlayerObjects;
            List<uint> touchedIds = new List<uint>();
            
            foreach (var player in ObjectTable.PlayerObjects)
            {
                TrackPlayer(player);
                touchedIds.Add(player.EntityId);
            }
            
            // Cleanup old/lost players
            if (playersToTrack.Count() != Players.Count)
            {
                // Run off the main thread
                Task.Run(() =>
                {
                    List<uint> keysToRemove = Players.Keys.AsValueEnumerable().Where(key => !touchedIds.Contains(key)).ToList();
                    foreach (var key in keysToRemove)
                    {
                        if (key == localPlayer.EntityId) continue;
                        Players.Remove(key);
                    }
                });
            }
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
    public readonly List<ITrigger> Triggers = [];
    public int SelectedTrigger = -1;
    
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
            PlayerUpdated(player, actor);
        }
        else
        {
            Player player = Players[actor.EntityId];
            player.Rotation = actor.Rotation;
            
            bool lastGrounded = player.Grounded;
            bool lastMounted = player.Mounted;
            
            player.UpdateState(actor);
            if (player.Position != actor.Position || lastGrounded != player.Grounded || lastMounted != player.Mounted)
            {
                PlayerUpdated(player, actor);
            }
        }
    }

    private void PlayerUpdated(Player player, IBattleChara actor)
    {
        Vector3 lastPos = player.Position;
        
        // Moved
        if (lastPos != actor.Position)
        {
            Vector3 lastVel = player.Velocity;
            player.Velocity = (actor.Position - lastPos) / (float)Framework.UpdateDelta.TotalSeconds;

            // Player jumped and their state was not changed.
            if (!player.Grounded && lastVel.Y <= 0 && player.Velocity.Y > 0)
            {
                // Might seem counter-intuitive, this will resolve the logic later correctly.
                player.Grounded = true;
            }
            
            player.Position = actor.Position;
        }

        Parallel.ForEach(Triggers, trigger =>
        {
            trigger.CheckCollision(player);
        });
    }

    private void CleanupPlayer(Player player)
    {
        // TODO: Remove player from all routes/triggers
    }

    public void Dispose()
    {
        Framework.Update -= Update;

        foreach (var player in Players.Values)
        {
            CleanupPlayer(player);
        }
        
        Players.Clear();
        Triggers.Clear();
        
        localPlayer = null;
    }
}
