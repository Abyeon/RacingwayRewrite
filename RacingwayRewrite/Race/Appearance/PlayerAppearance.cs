using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using MessagePack;
using RacingwayRewrite.Utils.Structs;

namespace RacingwayRewrite.Race.Appearance;

[MessagePackObject]
public class PlayerAppearance
{
    [Key(0)] public readonly byte[] Customize;
    [Key(1)] public readonly EquipmentModelId[] EquipmentModels;
    [Key(2)] public readonly Dictionary<byte, WeaponData[]> WeaponDictionary = new();
    [Key(3)] public readonly ushort Facewear;
    [Key(4)] public readonly bool IsHatHidden;
    [Key(5)] public readonly bool IsVisorToggled;
    [Key(6)] public readonly bool IsVieraEarsHidden;
    [Key(7)] public readonly bool IsWeaponHidden;

    public unsafe PlayerAppearance(BattleChara* character)
    {
        Customize = character->DrawData.CustomizeData.Data.ToArray();
        EquipmentModels = character->DrawData.EquipmentModelIds.ToArray();
        
        WeaponDictionary[character->ClassJob] = BuildWeaponData(character->DrawData.WeaponData.ToArray());
        Facewear = character->DrawData.GlassesIds[0];
        
        IsHatHidden = character->DrawData.IsHatHidden;
        IsVisorToggled = character->DrawData.IsVisorToggled;
        IsVieraEarsHidden = character->DrawData.VieraEarsHidden;
        IsWeaponHidden = character->DrawData.IsWeaponHidden;
    }

    public PlayerAppearance(
        byte[] customizeData, EquipmentModelId[] equipmentModels, Dictionary<byte, WeaponData[]> weaponDictionary, ushort facewear, bool isHatHidden, bool isVisorToggled, bool isVieraEarsHidden, bool isWeaponHidden)
    {
        Customize = customizeData;
        EquipmentModels = equipmentModels;
        WeaponDictionary = weaponDictionary;
        Facewear = facewear;
        IsHatHidden = isHatHidden;
        IsVisorToggled = isVisorToggled;
        IsVieraEarsHidden = isVieraEarsHidden;
        IsWeaponHidden = isWeaponHidden;
    }

    private static WeaponData[] BuildWeaponData(DrawObjectData[] data)
    {
        return
        [
            new WeaponData(0, data[0]),
            new WeaponData(1, data[1]),
            new WeaponData(2, data[2])
        ];
    }

    public ActorCustomize GetCustomizeData()
    {
        return new ActorCustomize()
        {
            Race = Customize[0],
            Sex = Customize[1],
            BodyType = Customize[2],
            Height = Customize[3],
            Tribe = Customize[4],
            FaceType = Customize[5],
            HairStyle = Customize[6],
            HasHighlights = Customize[7],
            SkinTone = Customize[8],
            RightEyeColor = Customize[9],
            HairColor = Customize[10],
            HairHighlightColor = Customize[11],
            FaceFeatures = Customize[12],
            FaceFeaturesColor = Customize[13],
            Eyebrows = Customize[14],
            LeftEyeColor = Customize[15],
            EyeShape = Customize[16],
            NoseShape = Customize[17],
            JawShape = Customize[18],
            LipStyle = Customize[19],
            LipColor = Customize[20],
            RaceFeatureSize = Customize[21],
            RaceFeatureType = Customize[22],
            BustSize = Customize[23],
            Facepaint = Customize[24],
            FacePaintColor =  Customize[25]
        };
    }
}
