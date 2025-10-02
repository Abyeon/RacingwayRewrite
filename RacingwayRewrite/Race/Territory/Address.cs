using MessagePack;
using Sheet = Lumina.Excel.Sheets;

namespace RacingwayRewrite.Race.Territory;

[MessagePackObject]
public readonly record struct Address(uint WorldId, uint TerritoryId, uint MapId, sbyte? Ward = null, sbyte? Plot = null, short? Room = null)
{
    [Key(0)] public readonly uint WorldId =  WorldId;
    [Key(1)] public readonly uint TerritoryId = TerritoryId;
    [Key(2)] public readonly uint MapId = MapId;
    [Key(3)] public readonly sbyte? Ward = Ward;
    [Key(4)] public readonly sbyte? Plot = Plot;
    [Key(5)] public readonly short? Room = Room;
    
    [IgnoreMember]
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

    [IgnoreMember]
    public string LifestreamCommand
    {
        get
        {
            if (Plot != null && Room != null && Ward != null)
            {
                var isAptWing = TerritoryTools.AptWings.ContainsKey((sbyte)Plot);
        
                // World, District, Ward, Subdivision / Plot, Room
                var command = Plugin.DataManager.GetExcelSheet<Sheet.World>().GetRow(WorldId).Name.ExtractText() + " " +
                              TerritoryTools.HousingDistricts[TerritoryTools.GetAreaRowId(TerritoryId)] +
                              $" w{Ward + 1}" +
                              (isAptWing ? (Plot == -128 ? "" : " s") + (Room == 0 ? "" : $" a{Room}") : $" p{Plot + 1}");

                return command;
            }
            
            // Just return the area name if not in a room/plot
            return TerritoryTools.GetAreaFromId(TerritoryId);
        }
    }

    public void TeleportWithLifestream()
    {
        Plugin.LifestreamIpcHandler.ExecuteCommand.InvokeAction(LifestreamCommand);
    }
}
