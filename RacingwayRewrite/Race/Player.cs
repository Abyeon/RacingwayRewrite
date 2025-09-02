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
    
    public IPlayerCharacter? Character { get; set; }
    public bool IsClient { get; set; } = false;
    public RaceState State { get; set; }
    
    public bool Grounded { get; set; } = true;
    public bool Mounted { get; set; } = false;
    public Vector3 LastPosition { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 LastVelocity { get; set; }
    public Vector3 Velocity { get; set; }
    public float Rotation { get; set; }

    public Player(IBattleChara actor)
    {
        if (actor is IPlayerCharacter character)
        {
            if (actor == Plugin.ClientState.LocalPlayer)
                IsClient = true;
            
            Id = character.EntityId;
            Name = character.Name.ToString();
            
            Character = character;
            
            Rotation = character.Rotation;
            HomeworldRow = character.HomeWorld.RowId;
            Position = character.Position;

            State = new RaceState(this);
        }
        else
        {
            Plugin.Log.Error($"Unable to add player {actor.EntityId}");
            throw new NullReferenceException("Player character was null");
        }
    }

    public unsafe void UpdateState(IBattleChara actor, float deltaTime)
    {
        Character* character = (Character*)actor.Address;
        Grounded = !character->IsJumping();
        Mounted = character->IsMounted();
        
        LastVelocity = Velocity;
        Velocity = (actor.Position - LastPosition) / deltaTime;
    }
}
