using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

using Sheet = Lumina.Excel.Sheets;

namespace RacingwayRewrite.Utils;

public class TerritoryTools
{
    //public Action AddressChanged;
    
    public TerritoryTools(IClientState clientState)
    {
        //AddressChanged = addressChanged;
        clientState.TerritoryChanged += TerritoryChanged;

        CheckLocationId();
    }
    
    public void TerritoryChanged(ushort id)
    {
        string? name = GetHousingName();
        if (name != null)
        {
            Plugin.Chat.Print("Territory Changed: " + id.ToString());
        }
        
        CheckLocationId();
    }

    private unsafe HouseInfo HousePoll()
    {
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
                Plugin.Chat.Print($"Ward: {result.Ward + 1}");
                Plugin.Chat.Print($"Plot: {result.Plot + 1}");
                Plugin.Chat.Print($"Room: {result.Room}");
            }
            else
            {
                Plugin.Chat.Print($"Could not find house info {result}");
            }
        }
        catch (Exception e)
        {
            Plugin.Chat.Error(e.Message);
            Plugin.Log.Error(e.Message);
        }
    }
    
    public unsafe void CheckLocationId()
    {
        var manager = HousingManager.Instance();
        bool inside = manager->IsInside();

        // Player is inside a house
        if (inside)
        {
            PollForPlot();
        }
    }
    
    private string? GetHousingName()
    {
        var housingTerritoryTypeId = HousingManager.GetOriginalHouseTerritoryTypeId();
        if (housingTerritoryTypeId == 0)
            return null;

        Plugin.DataManager.Excel.GetSheet<Sheet.TerritoryType>().TryGetRow(housingTerritoryTypeId, out var territory);

        return territory.PlaceName.ValueNullable?.Name.ExtractText();
    }
}
