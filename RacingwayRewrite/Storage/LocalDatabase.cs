using System;
using LiteDB;
using MessagePack;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Storage;

public class LocalDatabase : IDisposable
{
    internal readonly Plugin Plugin;
    
    private LiteDatabase db;

    public LocalDatabase(Plugin plugin, string path)
    {
        Plugin = plugin;
        db = new LiteDatabase($"filename={path};upgrade=true");
        
        // Register Address serialization using MessagePack- LiteDB serializer doesnt like uints or sbytes.
        BsonMapper.Global.RegisterType(serialize: address => new BsonValue(MessagePackSerializer.Serialize(address)), 
                                       deserialize: bson => MessagePackSerializer.Deserialize<Address>(bson.AsBinary));
        
        // Initiate Collections
        var routeCollection = GetRouteCollection();
        routeCollection.EnsureIndex(x => x.Address);
        routeCollection.EnsureIndex(x => x.Name);
        
        var recordCollection = GetRecordCollection();
        recordCollection.EnsureIndex(x => x.Time);
        recordCollection.EnsureIndex(x => x.Name);
    }

    public void Dispose()
    {
        db.Dispose();
    }

    internal RouteInfo[] RouteCache = [];
    
    internal ILiteCollection<RouteInfo> GetRouteCollection()
    {
        var collection = db.GetCollection<RouteInfo>("routes");
        RouteCache = collection.Query().ToArray(); // Update the cache every time the collection is acquired.
        
        return collection;
    }

    internal ILiteCollection<RecordInfo> GetRecordCollection()
    {
        return db.GetCollection<RecordInfo>("records");
    }
}
