namespace RacingwayRewrite.Race.Territory;

public record struct Address(uint WorldId, uint TerritoryId, uint MapId, sbyte? Ward = null, sbyte? Plot = null, short? Room = null)
{
    public readonly uint WorldId =  WorldId;
    public readonly uint TerritoryId = TerritoryId;
    public readonly uint MapId = MapId;
    public readonly sbyte? Ward = Ward;
    public readonly sbyte? Plot = Plot;
    public readonly short? Room = Room;
}
