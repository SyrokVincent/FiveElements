using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Enums;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;

namespace FiveElements.FiveElementsCode.Patches;

public static class LargeCapsuleSharedState
{
    public static CardElementTag? CurrentRandomElement;
}

[HarmonyPatch(typeof(LargeCapsule), nameof(LargeCapsule.AfterObtained))]
public static class LargeCapsuleMainPatch
{
    public static void Prefix(LargeCapsule __instance)
    {
        var values = Enum.GetValues(typeof(CardElementTag))
            .Cast<CardElementTag>()
            .Where(t => t != CardElementTag.Neutral) // On exclut Neutral
            .ToArray();
        
        LargeCapsuleSharedState.CurrentRandomElement = values[Rng.Chaotic.NextInt(values.Length)];
    }

    public static void Postfix()
    {
        // On nettoie après pour ne pas influencer d'autres capsules plus tard
        LargeCapsuleSharedState.CurrentRandomElement = null;
    }
}


[HarmonyPatch(typeof(LargeCapsule), "GetStrikeForCharacter")]
public static class LargeCapsuleStrikePatch
{
    public static bool Prefix(CharacterModel character, ref CardModel __result)
    {
        if (character is Character.FiveElements && LargeCapsuleSharedState.CurrentRandomElement.HasValue)
        {
            __result = LargeCapsuleSharedState.CurrentRandomElement.Value switch
            {
                CardElementTag.Water => (CardModel)ModelDb.Card<WaterStrike>(),
                CardElementTag.Wood  => (CardModel)ModelDb.Card<WoodStrike>(),
                CardElementTag.Fire  => (CardModel)ModelDb.Card<FireStrike>(),
                CardElementTag.Earth => (CardModel)ModelDb.Card<EarthStrike>(),
                CardElementTag.Metal => (CardModel)ModelDb.Card<MetalStrike>(),
                _ => __result
            };
            return false; 
        }
        return true;
    }
}

[HarmonyPatch(typeof(LargeCapsule), "GetDefendForCharacter")]
public static class LargeCapsuleDefendPatch
{
    public static bool Prefix(CharacterModel character, ref CardModel __result)
    {
        if (character is Character.FiveElements && LargeCapsuleSharedState.CurrentRandomElement.HasValue)
        {
            __result = LargeCapsuleSharedState.CurrentRandomElement.Value switch
            {
                CardElementTag.Water => (CardModel)ModelDb.Card<WaterDefend>(),
                CardElementTag.Wood  => (CardModel)ModelDb.Card<WoodDefend>(),
                CardElementTag.Fire  => (CardModel)ModelDb.Card<FireDefend>(),
                CardElementTag.Earth => (CardModel)ModelDb.Card<EarthDefend>(),
                CardElementTag.Metal => (CardModel)ModelDb.Card<MetalDefend>(),
                _ => __result
            };
            return false;
        }
        return true;
    }
}
