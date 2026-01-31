using FFXIVClientStructs.FFXIV.Client.Game.Character;
using MessagePack;
using RacingwayRewrite.Race.Appearance;

namespace RacingwayRewrite.Race.Replay;

// For recording the player's state during a specific frame.
[MessagePackObject]
public unsafe struct MomentData(ActorData actorData, byte classJob, double offset)
{
    [Key(0)] public ActorData ActorData = actorData;
    [Key(1)] public readonly byte ClassJob = classJob;
    [Key(2)] public readonly double Offset = offset;

    public MomentData(BattleChara* character, double offset) : this(new ActorData(character), character->ClassJob, offset) { }
}
