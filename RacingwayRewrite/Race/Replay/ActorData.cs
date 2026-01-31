using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using MessagePack;

namespace RacingwayRewrite.Race.Replay;

[MessagePackObject]
public unsafe struct ActorData(Vector3 position, float yaw, ushort animation)
{
    [Key(0)] public readonly Vector3 Position = position;
    [Key(1)] public readonly float Yaw = yaw;
    [Key(2)] public readonly ushort Animation = animation;

    public ActorData(BattleChara* character) : this(character->Position, character->Rotation, character->Timeline.TimelineSequencer.TimelineIds[0]) { }
}


