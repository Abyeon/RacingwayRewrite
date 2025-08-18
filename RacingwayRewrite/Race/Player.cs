using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using RacingwayRewrite.Utils;

namespace RacingwayRewrite.Race;

public class Player
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public uint HomeworldRow { get; set; }

    public bool Grounded { get; set; } = true;
    public bool Mounted { get; set; } = false;
    public Vector3 Position { get; set; }

    public Player(IBattleChara actor)
    {
        if (actor is IPlayerCharacter character)
        {
            Id = character.EntityId;
            Name = character.Name.ToString();
            HomeworldRow = character.HomeWorld.RowId;
            Position = character.Position;
            
            //Plugin.Log.Verbose($"Player {Name} added");
        }
        else
        {
            Plugin.Log.Error($"Unable to add player {actor.EntityId}");
            throw new NullReferenceException("Player character was null");
        }
    }

    public unsafe void UpdateState(IBattleChara actor)
    {
        Character* character = (Character*)actor.Address;
        Grounded = !character->IsJumping();
        Mounted = character->IsMounted();
    }
}
