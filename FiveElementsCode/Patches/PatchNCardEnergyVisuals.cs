using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Godot;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;

namespace FiveElements.FiveElementsCode.Patches;

[HarmonyPatch(typeof(NCard), "UpdateEnergyCostVisuals")]
public class PatchNCardEnergyVisuals
{
    public static void Postfix(NCard __instance)
    {
        if (__instance == null || __instance.Model == null) return;

        var cardModel = __instance.Model;

        if (cardModel.Pool is FiveElementsCardPool elementPool)
        {
            var energyIconNode = __instance.GetNodeOrNull<TextureRect>("CardContainer/EnergyIcon");
            if (energyIconNode == null) return;

            SortedSet<CardElementTag> currentEcho = [CardElementTag.Neutral];

            if (cardModel.IsCanonical)
            {
                currentEcho = [CardElementTag.Neutral];
            } 
            else if (cardModel.Owner?.Creature != null)
            {
                currentEcho = cardModel.Owner.Creature.GetElementalStatus().Echo;
            }
            
            // Chargement de la texture
            string path = elementPool.GetEnergyPathForCard(currentEcho).ImagePath();
            if (!string.IsNullOrEmpty(path))
            {
                var newTexture = ResourceLoader.Load<Texture2D>(path);
                if (newTexture != null)
                {
                    energyIconNode.Texture = newTexture;
                }
            }
        }
    }
}