using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Cards._6_Ancient;
using FiveElements.FiveElementsCode.Enums;
using System.Collections.Generic;
using System.Linq;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Models;

[HarmonyPatch(typeof(CardModel), "get_HoverTips")]
public static class PatchHoverTips
{
    public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        // On cible Activation et Incarnation
        if (__instance is Activation || __instance is Incarnation)
        {
            // Si on n'est pas en combat, on laisse tout (pour la bibliothèque)
            if (__instance.CombatState == null) return;

            // Si on est en combat mais que la Terre est OFF, on dégage le bloc de FORCE
            if (!CardElementTag.Earth.IsActive(__instance.Owner.Creature))
            {
                string blockId = HoverTipFactory.Static(StaticHoverTip.Block).Id;
                __result = __result.Where(tip => tip.Id != blockId);
            }
        }
    }
}