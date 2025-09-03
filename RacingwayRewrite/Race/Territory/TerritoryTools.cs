using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using RacingwayRewrite.Structs;
using RacingwayRewrite.Utils;
using Sheet = Lumina.Excel.Sheets;

namespace RacingwayRewrite.Race.Territory;

public class TerritoryTools
{
    public event EventHandler<Address>? OnAddressChanged;
    internal readonly Plugin Plugin;
    internal readonly IClientState ClientState;
    
    public TerritoryTools(Plugin plugin, IClientState clientState)
    {
        Plugin = plugin;
        ClientState = clientState;
        
        clientState.TerritoryChanged += TerritoryChanged;

        if (clientState.IsLoggedIn)
            CheckLocation();
    }
    
    private void TerritoryChanged(ushort id)
    {
        CheckLocation();
    }
    
    public static readonly Dictionary<uint, string> HousingDistricts = new()
    {
        {502, "Mist"},
        {505, "Goblet"},
        {507, "Lavender Beds"},
        {512, "Empyreum"},
        {513, "Shirogane"}
    };
    
    public static readonly Dictionary<sbyte, string> AptWings = new()
    {
        {-128, "wing 1" },
        {-127, "wing 2" }
    };
    
    public unsafe uint TerritoryId
    {
        get
        {
            var agent = AgentMap.Instance();
            return agent->CurrentTerritoryId;
        }
    }
    
    public unsafe uint MapId
    {
        get
        {
            var agent = AgentMap.Instance();
            return agent->CurrentMapId;
        }
    }
    
    public string AreaName => GetAreaFromId(CorrectedTerritoryTypeId);

    public static string GetAreaFromId(uint id)
    {
        try
        {
            return Plugin.DataManager.GetExcelSheet<Sheet.TerritoryType>().GetRow(id).PlaceName.Value.Name.ExtractText();
        }
        catch (Exception e)
        {
            Plugin.Chat.Error(e.Message);
            if (e.StackTrace != null) Plugin.Log.Error(e.StackTrace);
        }

        return "UNKNOWN";
    }

    public static uint GetAreaRowId(uint id)
    {
        try
        {
            return Plugin.DataManager.GetExcelSheet<Sheet.TerritoryType>().GetRow(id).PlaceNameZone.RowId;
        }
        catch (Exception e)
        {
            Plugin.Chat.Error(e.Message);
            if (e.StackTrace != null) Plugin.Log.Error(e.StackTrace);
        }

        return 0;
    }
    
    // Thanks to ICritical
    //https://github.com/Critical-Impact/CriticalCommonLib/blob/bc358bd4acb1ce8110e51e9eaa495ff12a0300bc/Services/CharacterMonitor.cs#L899
    private unsafe uint CorrectedTerritoryTypeId
    { 
        get
        {
            var manager = HousingManager.Instance();
            if (manager == null)
            {
                return Plugin.ClientState.TerritoryType;
            }
    
            var character = Plugin.ClientState.LocalPlayer;
            if (character != null && manager->CurrentTerritory != null)
            {
                var territoryType = manager->IndoorTerritory != null
                                        ? ((HousingTerritory2*)manager->CurrentTerritory)->TerritoryTypeId
                                        : Plugin.ClientState.TerritoryType;

                return territoryType;
            }
    
            return Plugin.ClientState.TerritoryType;
        }
    }

    private uint World
    {
        get
        {
            if (ClientState.LocalPlayer == null)
                throw new NullReferenceException("Tried to get the player's world while they do not exist!");
            
            return ClientState.LocalPlayer.CurrentWorld.RowId;
        }
    }
    
    private unsafe HouseInfo HousePoll()
    {
        if (ClientState.LocalPlayer == null) return new HouseInfo(-1, -1, -1);
        
        var manager = HousingManager.Instance();
        var ward = manager->GetCurrentWard();
        var plot = manager->GetCurrentPlot();
        var room = manager->GetCurrentRoom();
        
        return new HouseInfo(ward, plot, room);
    }
    
    private async void PollForPlot()
    {
        try
        {
            HouseInfo? result = await Plugin.Framework.PollForValue(HousePoll, (value) => value.Plot != -1, 50);
            if (result != null && result.Plot != -1)
            {
                OnAddressChanged?.Invoke(this, new Address(World, CorrectedTerritoryTypeId, MapId, result.Ward, result.Plot, result.Room));
            }
            else
            {
                Plugin.Chat.Error($"Could not find house info {result}");
            }
        }
        catch (Exception e)
        {
            Plugin.Chat.Error(e.Message);
            Plugin.Log.Error(e.Message);
        }
    }

    private async void PollForOutside()
    {
        try
        {
            bool playerOutside = await Plugin.Framework.PollForValue(() =>
            {
                return ClientState.LocalPlayer != null;
            }, outside => outside, 50, 10000);

            if (playerOutside)
            {
                OnAddressChanged?.Invoke(this, new Address(World, CorrectedTerritoryTypeId, MapId));
            }
        }
        catch (Exception e)
        {
            Plugin.Chat.Error(e.Message);
            Plugin.Log.Error(e.Message);
        }
    }
    
    public unsafe void CheckLocation()
    {
        var manager = HousingManager.Instance();
        bool inside = manager->IsInside();

        // Player is inside a house
        if (inside)
        {
            PollForPlot();
        }
        else
        {
            PollForOutside();
        }
    }
}
