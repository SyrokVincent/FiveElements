using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Cards._6_Ancient;
using FiveElements.FiveElementsCode.Enums;
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
            // 2. Sécurité absolue : si c'est un modèle canonique (Bibliothèque/Compendium),
            // on ne touche à rien et on ne regarde surtout pas l'Owner.
            if (__instance.IsCanonical) return;
            
            // 3. Sécurité CRITIQUE pour Orobas/ArchaicTooth : 
            // On vérifie que la carte appartient bien à quelqu'un AVANT de checker l'élément
            if (__instance.Owner?.Creature == null) 
            {
                // Si pas de propriétaire, on ne filtre rien pour éviter le crash
                return; 
            }
            // Si la Terre est OFF, on dégage le bloc de FORCE
            if (!CardElementTag.Earth.IsActive(__instance.Owner.Creature))
            {
                string blockId = HoverTipFactory.Static(StaticHoverTip.Block).Id;
                __result = __result.Where(tip => tip.Id != blockId);
            }
        }
    }
}