using System;
using System.Numerics;
using MessagePack;
using RacingwayRewrite.Race.Replay;

namespace RacingwayRewrite.Race;

[MessagePackObject]
public class Record(ulong contentId, string name, string world, DateTime created, TimeSpan time, MomentData[] moments)
{
    [Key(0)] public ulong ContentId { get; set; } = contentId;
    [Key(1)] public string Name { get; set; } = name;
    [Key(2)] public string World { get; set; } = world;
    [Key(3)] public DateTime Created { get; set; } = created;
    [Key(4)] public TimeSpan Time { get; set; } = time;
    [Key(5)] public MomentData[] Moments { get; set; } = moments;

    public Record(ulong contentId, string name, string world, TimeSpan time, MomentData[] moments) : this(contentId, name, world, DateTime.UtcNow, time, moments) { }
}
