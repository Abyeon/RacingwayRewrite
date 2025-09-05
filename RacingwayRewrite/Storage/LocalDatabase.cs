using System;
using LiteDB;

namespace RacingwayRewrite.Storage;

public class LocalDatabase : IDisposable
{
    internal readonly Plugin Plugin;
    
    private LiteDatabase db;

    public LocalDatabase(Plugin plugin, string path)
    {
        Plugin = plugin;
        db = new LiteDatabase($"filename={path};upgrade=true");
        
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

    internal ILiteCollection<RouteInfo> GetRouteCollection()
    {
        return db.GetCollection<RouteInfo>("routes");
    }

    internal ILiteCollection<RecordInfo> GetRecordCollection()
    {
        return db.GetCollection<RecordInfo>("records");
    }
}
