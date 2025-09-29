using LiteDB;
using MessagePack;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Storage;

/// <summary>
/// Small packed version of a Route
/// </summary>
public struct RouteInfo(string name, string author, string description, Address address, byte[] packedRoute)
{
    public ObjectId Id { get; set; } = new();
    public string Name { get; set; } = name;
    public string Author { get; set; } = author;
    public string Description { get; set; } = description;
    public Address Address { get; set; } = address;
    public byte[] PackedRoute { get; set; } = packedRoute;

    [BsonIgnore]
    public Route Route => MessagePackSerializer.Deserialize<Route>(PackedRoute);
}
