using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Race.Collision.Triggers;
using RacingwayRewrite.Race.Territory;

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
        
        TerritoryTools = Plugin.TerritoryTools;
        TerritoryTools.OnAddressChanged += AddressChanged;
        ClientState.Logout += OnLogout;
        ClientState.EnterPvP += OnEnterPvp;
        
        // Reload current area
        if (TerritoryTools.CurrentAddress != null)
            LoadRoutes((Address)TerritoryTools.CurrentAddress);
    }

    private void OnEnterPvp() => UnloadRoutes();
    private void OnLogout(int type, int code) => UnloadRoutes();

    private void AddressChanged(object? sender, Address e)
    {
        CurrentAddress = e;
        
        LoadRoutes(e);
    }

    public void LoadRoutes(Address address)
    {
        // Run off the main thread to avoid hangs when loading
        Task.Run(() =>
        {
            UnloadRoutes();
            
            if (Plugin.Storage == null)
            {
                Plugin.Chat.Warning("Unable to load routes when Storage is null.");
                return;
            }
        
            var routeCollection = Plugin.Storage.GetRouteCollection();
            var routes = routeCollection.Query().Where(x => x.Address == address).ToList();

            foreach (var packed in routes)
            {
                var route = packed.Route;
                //route.Id = packed.Id;
                
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

    public void UnloadRoutes()
    {
        if (LoadedRoutes.Count == 0) return;
        SelectedRoute = -1;
        
        // Kick players out of their race
        Plugin.RaceManager.KickAllPlayers();
        
        SaveRoutes();
        CurrentAddress = null;
        
        LoadedRoutes = [];
    }

    public void SaveRoutes()
    {
        if (Plugin.Storage == null) return;
        
        // Ensure routes get saved into the database
        foreach (var route in LoadedRoutes)
        {
            Plugin.Storage.SaveRoute(route);
        }
    }
    
    public void Dispose()
    {
        TerritoryTools.OnAddressChanged -= AddressChanged;
        ClientState.Logout -= OnLogout;
        ClientState.EnterPvP -= OnEnterPvp;
        
        UnloadRoutes();
    }
}
