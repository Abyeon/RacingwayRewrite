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
                return Plugin.DataManager.GetExcelSheet<Sheet.World>().GetRow(WorldId).Name.ExtractText() + " " +
                       TerritoryTools.HousingDistricts[TerritoryTools.GetAreaRowId(TerritoryId)] +
                       $" w{Ward+1}" +
                       $" p{Plot+1}" +
                       (Room == 0 ? "" : $" room {Room}");
            }
        
            return TerritoryTools.GetAreaFromId(TerritoryId);
        }
    }
}
