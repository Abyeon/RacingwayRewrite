using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using MessagePack;

namespace RacingwayRewrite.Race.Appearance;

[MessagePackObject]
public unsafe struct ActorData(BattleChara* character)
{
    [Key(0)] public readonly Vector3 Position = character->Position;
    [Key(1)] public readonly float Yaw = character->Rotation;
    [Key(2)] public readonly ushort Animation = character->Timeline.TimelineSequencer.TimelineIds[0];
}


