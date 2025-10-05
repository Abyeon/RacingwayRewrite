using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace RacingwayRewrite.Race;

public unsafe struct ActorData(BattleChara* character)
{
    public Vector3 Position = character->Position;
    public float Yaw = character->Rotation;
    public EffectContainer Effects = character->Effects;
    public TimelineContainer Timeline = character->Timeline;
}


