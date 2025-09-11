using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using LiteDB;
using MessagePack;
using RacingwayRewrite.Race.Collision;
using RacingwayRewrite.Race.Territory;
using RacingwayRewrite.Storage;

namespace RacingwayRewrite.Race;

public class RouteLoader : IDisposable
{
    internal readonly Plugin Plugin;
    internal readonly IClientState ClientState;
    internal static TerritoryTools TerritoryTools { get; private set; } = null!;
    
    public Address? CurrentAddress { get; set; }
    public int SelectedRoute { get; internal set; } = -1;
    public ITrigger? SelectedTrigger { get; internal set; }
    public ITrigger? HoveredTrigger { get; internal set; }
    public List<Route> LoadedRoutes { get; private set; } = [];

    public RouteLoader(Plugin plugin, IClientState clientState)
    {
        Plugin = plugin;
        ClientState = clientState;
        
        TerritoryTools = new TerritoryTools(plugin, ClientState);
        TerritoryTools.OnAddressChanged += AddressChanged;
        ClientState.Logout += OnLogout;
        ClientState.EnterPvP += OnEnterPvp;
    }

    private void OnEnterPvp() => UnloadRoutes();
    private void OnLogout(int type, int code) => UnloadRoutes();

    private void AddressChanged(object? sender, Address e)
    {
        CurrentAddress = e;
        
        UnloadRoutes();
        LoadRoutes(e);
    }

    public void LoadRoutes(Address address)
    {
        // Run off the main thread to avoid hangs when loading
        Task.Run(() =>
        {
            if (Plugin.Storage == null)
            {
                Plugin.Chat.Warning("Unable to load routes when Storage is null.");
                return;
            }
        
            ILiteCollection<RouteInfo> routeCollection = Plugin.Storage.GetRouteCollection();
            List<RouteInfo> routes = routeCollection.Query().Where(x => x.Address == address).ToList();

            foreach (var packed in routes)
            {
                Route route = MessagePackSerializer.Deserialize<Route>(packed.PackedRoute);
                route.Id = packed.Id;
                
                // Update start trigger to reference route
                if (route.Triggers.Exists(x => x is Start))
                {
                    Start? start = (Start?)route.Triggers.Find(x => x is Start);
                    if (start != null)
                    {
                        start.Route = route;
                    }
                }
                
                // Do the same for loops
                if (route.Triggers.Exists(x => x is Loop))
                {
                    Loop? loop = (Loop?)route.Triggers.Find(x => x is Loop);
                    if (loop != null)
                    {
                        loop.Route = route;
                    }
                }
                
                LoadedRoutes.Add(route);
            }
            
            // Pre-sort routes by name
            LoadedRoutes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        
            if (LoadedRoutes.Count > 0)
                Plugin.Chat.Print($"Loaded {LoadedRoutes.Count} route(s).");
        });
    }

    private void UnloadRoutes()
    {
        if (Plugin.Storage == null) return;
        
        SelectedRoute = -1;
        if (!LoadedRoutes.Any()) return;
        
        // Ensure routes get saved into the database
        ILiteCollection<RouteInfo> routeCollection = Plugin.Storage.GetRouteCollection();
        foreach (Route route in LoadedRoutes)
        {
            byte[] packed = MessagePackSerializer.Serialize(route);
            
            // This is a new route, not saved in the database
            if (route.Id == null)
            {
                RouteInfo toSave = new RouteInfo(route.Name, route.Description, route.Address, packed);
                routeCollection.Insert(toSave);
                continue;
            }
            
            // Route exists, just update the entry
            routeCollection.Update(route.Id, new RouteInfo(route.Name, route.Description, route.Address, packed));
        }
        
        LoadedRoutes = [];
    }
    
    public void Dispose()
    {
        TerritoryTools.OnAddressChanged -= AddressChanged;
        ClientState.Logout -= OnLogout;
        ClientState.EnterPvP -= OnEnterPvp;
        
        UnloadRoutes();
    }
}
