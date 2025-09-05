using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Race.Territory;

namespace RacingwayRewrite.Race;

public class RouteManager : IDisposable
{
    internal readonly Plugin Plugin;
    internal readonly IClientState ClientState;
    internal static TerritoryTools TerritoryTools { get; private set; } = null!;
    
    public IEnumerable<Route> LoadedRoutes { get; private set; } = [];

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
    }

    // public async void LoadRoutes(Address address)
    // {
    //     
    // }

    private void UnloadRoutes()
    {
        if (!LoadedRoutes.Any()) return;
        // TODO: Probably save routes to disk here if they've changed
        
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
