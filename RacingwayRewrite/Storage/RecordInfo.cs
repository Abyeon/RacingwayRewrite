using System;
using LiteDB;

namespace RacingwayRewrite.Storage;

/// <summary>
/// Packed version of Records
/// </summary>
public struct RecordInfo(AppearanceInfo appearance, string name, string world, DateTime created, TimeSpan time, byte[] packedRecord)
{
    public required ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary>
    /// Reference ID to an AppearanceInfo with this player's data.
    /// </summary>
    [BsonRef("appearances")]
    public required AppearanceInfo Appearance { get; set; } = appearance;

    public required string Name { get; init; } = name;
    public required string World { get; init; } = world;
    public required DateTime Created { get; init; } = created;
    public required TimeSpan Time { get; init; } = time;
    public required byte[] PackedRecord { get; init; } = packedRecord;
}
