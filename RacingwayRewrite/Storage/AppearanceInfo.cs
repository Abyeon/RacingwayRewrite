using LiteDB;

namespace RacingwayRewrite.Storage;

public struct AppearanceInfo (ulong contentId, byte[] packedAppearance)
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public ulong ContentId { get; set; } = contentId;
    public byte[] PackedAppearance { get; set; } = packedAppearance;
}
