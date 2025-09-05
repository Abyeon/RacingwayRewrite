using System;
using System.Numerics;

namespace RacingwayRewrite.Race;

public class Record(string name, string world, TimeSpan time, Vector3[] line)
{
    public string Name { get; set; } = name;
    public string World { get; set; } = world;
    public TimeSpan Time { get; set; } = time;
    public Vector3[] Line { get; set; } = line;
}
