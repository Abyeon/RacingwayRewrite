using LiteDB;

namespace RacingwayRewrite.Storage;

public struct AppearanceInfo
{
    public ObjectId Id { get; set; }
    public required ulong ContentId { get; set; }
    public byte[] PackedAppearance { get; set; }
}
