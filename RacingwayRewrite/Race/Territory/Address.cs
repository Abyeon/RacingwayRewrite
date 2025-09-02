using Sheet = Lumina.Excel.Sheets;

namespace RacingwayRewrite.Race.Territory;

public readonly record struct Address(uint WorldId, uint TerritoryId, uint MapId, sbyte? Ward = null, sbyte? Plot = null, short? Room = null)
{
    public readonly uint WorldId =  WorldId;
    public readonly uint TerritoryId = TerritoryId;
    public readonly uint MapId = MapId;
    public readonly sbyte? Ward = Ward;
    public readonly sbyte? Plot = Plot;
    public readonly short? Room = Room;
    
    public string ReadableName
    {
        get
        {
            if (Plot != null && Room != null && Ward != null)
            {
                // World, District, Ward, Apartment Wing / Plot, Room
                return Plugin.DataManager.GetExcelSheet<Sheet.World>().GetRow(WorldId).Name.ExtractText() + " " +
                       TerritoryTools.HousingDistricts[TerritoryTools.GetAreaRowId(TerritoryId)] +
                       $" w{Ward+1}" +
                       (TerritoryTools.AptWings.ContainsKey((sbyte)Plot) ? $" {TerritoryTools.AptWings[(sbyte)Plot]}" : $" p{Plot+1}") +
                       (Room == 0 ? "" : $" room {Room}");
            }
        
            // Just return the area name if not in a room/plot
            return TerritoryTools.GetAreaFromId(TerritoryId);
        }
    }
}
