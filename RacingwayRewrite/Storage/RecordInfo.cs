using System;
using LiteDB;

namespace RacingwayRewrite.Storage;

public class RecordInfo
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public string World { get; set; }
    public TimeSpan Time { get; set; }
    public byte[] PackedRecord { get; set; }
}
