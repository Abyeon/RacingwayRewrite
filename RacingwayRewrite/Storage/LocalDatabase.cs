using System;
using System.IO;
using System.Linq;
using LiteDB;
using MessagePack;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Appearance;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Storage;

public class LocalDatabase : IDisposable
{
    internal readonly Plugin Plugin;

    private string dbPath;
    private LiteDatabase db;

    public LocalDatabase(Plugin plugin, string path)
    {
        Plugin = plugin;
        dbPath = path;
        db = new LiteDatabase($"filename={path};upgrade=true");
        
        // Register Address serialization using MessagePack- LiteDB serializer doesnt like uints or sbytes.
        BsonMapper.Global.RegisterType(serialize: address => new BsonValue(MessagePackSerializer.Serialize(address)), 
                                       deserialize: bson => MessagePackSerializer.Deserialize<Address>(bson.AsBinary));
        
        // Initiate Collections
        var routeCollection = GetRouteCollection();
        routeCollection.EnsureIndex(x => x.Address);
        routeCollection.EnsureIndex(x => x.Name);
        
        var appearanceCollection = GetAppearanceCollection();
        appearanceCollection.EnsureIndex(x => x.ContentId);
        
        var recordCollection = GetRecordCollection();
        recordCollection.EnsureIndex(x => x.Time);
        recordCollection.EnsureIndex(x => x.Created);
        recordCollection.EnsureIndex(x => x.Name);
    }
    
    public void Dispose()
    {
        db.Dispose();
        GC.SuppressFinalize(this);
    }

    public string FileSize
    {
        get
        {
            try
            {
                var info = new FileInfo(dbPath);
                string[] suffixes = ["B", "KB", "MB", "GB", "TB"];

                int i = 0;
                decimal dValue = info.Length;
                while (Math.Round(dValue / 1024) >= 1)
                {
                    dValue /= 1024;
                    i++;
                }

                return $"{dValue:n1} {suffixes[i]}";
            }
            catch
            {
                return "Error";
            }
        }
    }

    internal RouteInfo[] RouteCache = [];
    
    internal ILiteCollection<RouteInfo> GetRouteCollection()
    {
        var collection = db.GetCollection<RouteInfo>("routes");
        RouteCache = collection.Query().ToArray(); // Update the cache every time the collection is acquired.
        
        return collection;
    }

    internal ILiteCollection<AppearanceInfo> GetAppearanceCollection()
    {
        return db.GetCollection<AppearanceInfo>("appearances");
    }

    internal ILiteCollection<RecordInfo> GetRecordCollection()
    {
        return db.GetCollection<RecordInfo>("records");
    }
    
    internal void SaveAppearance(PlayerAppearance appearance)
    {
        var appearanceCollection = GetAppearanceCollection();
        
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
        byte[] packed = MessagePackSerializer.Serialize(appearance, lz4Options);
        var toSave = new AppearanceInfo(appearance.ContentId, packed)
        {
            Id = appearance.Id
        };
    }

    internal void SaveRecord(Record record)
    {
        var recordCollection = GetRecordCollection();
        var appearanceCollection = GetAppearanceCollection();
        
        var exists = appearanceCollection.Query().Where(x => x.ContentId == record.ContentId);
        
        if (exists.Count() == 0)
        {
            Plugin.Chat.Error("Error saving record, player appearance not found! This is weird...");
            return;
        }
        
        var appearance = exists.First();
        
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
        byte[] packed = MessagePackSerializer.Serialize(record, lz4Options);
        // var toSave = new RecordInfo(appearance, record.Name, record.World, record.Created, record.Time, packed)
        // {
        //     Id = record
        // };
    }

    internal void SaveRoute(Route route, bool reload = false)
    {
        var routeCollection = GetRouteCollection();
            
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
        byte[] packed = MessagePackSerializer.Serialize(route, lz4Options);
        var toSave = new RouteInfo(route.Name, route.Author, route.Description, route.Address, packed)
        {
            Id = route.Id
        };
        
        try
        {
            // Update entry or insert a new one
            routeCollection.Upsert(route.Id, toSave);
            RouteCache = routeCollection.Query().ToArray();
            
            var currAddress = Plugin.TerritoryTools.CurrentAddress;
            
            // Add route to currently loaded routes
            if (reload && currAddress != null && (Address)currAddress == route.Address)
            {
                var loader = Plugin.RaceManager.RouteLoader;
                
                // Update route in collection
                loader.LoadedRoutes.RemoveAll(x => x.Id == route.Id);
                loader.LoadedRoutes.Add(route);
                
                Plugin.Chat.Print($"Saved and loaded {route.Name}.");
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e.ToString());
        }
    }

    internal void DeleteRoute(ObjectId? id)
    {
        var loader = Plugin.RaceManager.RouteLoader;
        var cacheIndex = Array.FindIndex(RouteCache, x => x.Id == id);
        
        var route = RouteCache.FirstOrDefault(x => x.Id == id);
        
        // If the route is loaded, delete it from the loaded routes
        int index = loader.LoadedRoutes.FindIndex(x => x.Id == id);
        if (index != -1)
        {
            var loadedRoute = loader.LoadedRoutes[index];
            
            // Kick every player just in case
            foreach (var player in Plugin.RaceManager.Players.Values)
            {
                if (player.State.CurrentRoute?.Id == id)
                {
                    player.State.Fail("Route was deleted!");
                    loadedRoute.Kick(player);
                }
            }
            
            // Delete from loader
            loader.LoadedRoutes.RemoveAt(index);
            if (loader.SelectedRoute == index)
                loader.SelectedRoute = -1;
        }
        
        var routeCollection = GetRouteCollection();
                    
        // If the route exists in DB, delete it
        if (route.Id != null && routeCollection.Exists(x => x.Id == route.Id!))
        {
            routeCollection.Delete(route.Id);
        }
        
        string name = route.Name;
        
        // Delete from the cache
        if (cacheIndex != -1)
        {
            var list = RouteCache.ToList();
            list.RemoveAt(cacheIndex);
            RouteCache = list.ToArray();
        }
                        
        Plugin.Chat.Print($"Deleted {name} from storage.");
    }
}
