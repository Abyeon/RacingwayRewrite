using FFXIVClientStructs.FFXIV.Client.Game.Character;
using MessagePack;

namespace RacingwayRewrite.Race.Appearance;

[MessagePackObject]
public class WeaponData
{
    [Key(0)] public readonly uint Slot;
    [Key(1)] public readonly ushort Id;
    [Key(2)] public readonly ushort Type;
    [Key(3)] public readonly ushort Variant;
    [Key(4)] public readonly byte Stain0;
    [Key(5)] public readonly byte Stain1;
    [Key(6)] public readonly ulong Value;
    
    [Key(7)] public readonly byte State;
    [Key(8)] public readonly ushort Flags1;
    [Key(9)] public readonly byte Flags2;

    public WeaponData(uint slot, DrawObjectData data)
    {
        Slot = slot;
        
        var model = data.ModelId;
        Id = model.Id;
        Type = model.Type;
        Variant = model.Variant;
        Stain0 = model.Stain0;
        Stain1 = model.Stain1;
        Value = model.Value;
        
        State = data.State;
        Flags1 = data.Flags1;
        Flags2 = data.Flags2;
    }

    public WeaponData(
        uint slot, ushort id, ushort type, ushort variant, byte stain0, byte stain1, ulong value, byte state, ushort flags1, byte flags2)
    {
        Slot = slot;
        
        Id = id;
        Type = type;
        Variant = variant;
        Stain0 = stain0;
        Stain1 = stain1;
        Value = value;
        
        State = state;
        Flags1 = flags1;
        Flags2 = flags2;
    }
    
    public WeaponModelId GetWeapon()
    {
        var weapon = new WeaponModelId
        {
            Id = Id,
            Type = Type,
            Variant = Variant,
            Stain0 = Stain0,
            Stain1 = Stain1,
            Value = Value
        };

        return weapon;
    }

    /// <summary>
    /// Loads this weapon onto the character provided.
    /// </summary>
    /// <param name="character">Character to load the weapon on.</param>
    public unsafe void LoadToCharacter(BattleChara* character)
    {
        var slot = (DrawDataContainer.WeaponSlot)Slot;
        character->DrawData.LoadWeapon(slot, GetWeapon(), 0, 0, 0, 0);
        
        ref var data = ref character->DrawData.Weapon(slot);
        data.State = State;
        data.Flags1 = Flags1;
        data.Flags2 = Flags2;
    }
}
