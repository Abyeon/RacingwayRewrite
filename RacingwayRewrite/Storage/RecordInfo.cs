using System;
using LiteDB;

namespace RacingwayRewrite.Storage;

public struct RecordInfo
{
    public required ObjectId Id { get; set; }
    public required string Name { get; set; }
    public required string World { get; set; }
    public required TimeSpan Time { get; set; }
    public required byte[] PackedRecord { get; set; }
}
