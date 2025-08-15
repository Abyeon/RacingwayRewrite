using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

namespace RacingwayRewrite.Race;

public class Player
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public uint HomeworldRow { get; set; }
    public Vector3 Position { get; set; }

    public Player(IBattleChara actor)
    {
        IPlayerCharacter? character = actor as IPlayerCharacter;
        if (character != null)
        {
            this.Id = character.EntityId;
            this.Name = character.Name.ToString();
            this.HomeworldRow = character.HomeWorld.RowId;
            this.Position = character.Position;
            Plugin.Log.Debug($"Player {this.Name} added");
        }
        else
        {
            Plugin.Log.Error($"Unable to add player {actor.EntityId}");
            throw new NullReferenceException("Player character was null");
        }
    }
}
