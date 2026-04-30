using BaseLib.Abstracts;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Patches;

public class TemporaryPatch
{
    //todo remove this and the 3 line in mainfile when baselib upgrade and fix the problem
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.BannerMaterial), MethodType.Getter)]
    class FixCustomCardBannerMaterial
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        static bool Prefix(CardModel __instance, ref Material? __result)
        {
            // 1. On vérifie si c'est une CustomCardModel
            if (__instance is CustomCardModel customCard)
            {
                // 4. SI customMat est null, on laisse le flux continuer.
                // MAIS pour éviter que la BaseLib n'applique son erreur (FrameMaterial), 
                // on doit empêcher son exécution spécifique.
            }
            return true;
        }
    }
}