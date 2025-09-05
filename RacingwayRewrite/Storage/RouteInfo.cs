using LiteDB;
using MessagePack;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Storage;

/// <summary>
/// Small packed version of a Route
/// </summary>
public class RouteInfo
{
    public RouteInfo(string name, string description, Address address, byte[] packedRoute)
    {
        Name = name;
        Description = description;
        Address = address;
        PackedRoute = packedRoute;
    }

    public ObjectId Id { get; set; } = new ObjectId();
    public string Name { get; set; }
    public string Description { get; set; }
    public Address Address { get; set; }
    public byte[] PackedRoute { get; set; }

    [BsonIgnore]
    Route Route => MessagePackSerializer.Deserialize<Route>(PackedRoute);
}
