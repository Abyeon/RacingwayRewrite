using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using LiteDB;
using MessagePack;
using RacingwayRewrite.Race.Territory;
using RacingwayRewrite.Storage;

namespace RacingwayRewrite.Race;

public class RouteManager : IDisposable
{
    internal readonly Plugin Plugin;
    internal readonly IClientState ClientState;
    internal static TerritoryTools TerritoryTools { get; private set; } = null!;
    
    public Address? CurrentAddress { get; set; }
    public Route? SelectedRoute { get; internal set; }
    public List<Route> LoadedRoutes { get; private set; } = [];

    public RouteManager(Plugin plugin, IClientState clientState)
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
        Plugin.Chat.Print(e.ReadableName);
        CurrentAddress = e;
        
        UnloadRoutes();
        LoadRoutes(e);
    }

    public void LoadRoutes(Address address)
    {
        Task.Run(() =>
        {
            if (Plugin.Storage == null)
            {
                Plugin.Chat.Warning("Unable to load routes when Storage is null.");
                return;
            }
        
            ILiteCollection<RouteInfo> routeCollection = Plugin.Storage.GetRouteCollection();
            List<RouteInfo> routes = routeCollection.Query().Where(x => x.Address == address).ToList();

            Parallel.ForEach(routes, packed =>
            {
                Route route = MessagePackSerializer.Deserialize<Route>(packed.PackedRoute);
                route.Id = packed.Id;
                LoadedRoutes.Add(route);
            });
        
            if (LoadedRoutes.Count > 0)
                Plugin.Chat.Print($"Loaded {LoadedRoutes.Count} routes.");
        });
    }

    private void UnloadRoutes()
    {
        if (Plugin.Storage == null) return;
        
        SelectedRoute = null;
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
