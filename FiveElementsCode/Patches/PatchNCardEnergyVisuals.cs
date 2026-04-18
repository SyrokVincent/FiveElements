using FiveElements.FiveElementsCode.Character;

namespace FiveElements.FiveElementsCode.Patches;

using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Godot;

[HarmonyPatch(typeof(NCard), "UpdateEnergyCostVisuals")]
public class PatchNCardEnergyVisuals
{
    public static void Postfix(NCard __instance)
    {
        var cardModel = __instance.Model;

        if (cardModel != null && cardModel.Pool is FiveElementsCardPool elementPool)
        {
            
            var energyIconNode = __instance.GetNodeOrNull<TextureRect>("CardContainer/EnergyIcon");
            
            if (energyIconNode != null)
            {
                // On force la texture
                var newTexture = ResourceLoader.Load<Texture2D>(elementPool.BigEnergyIconPath);
                energyIconNode.Texture = newTexture;
                
                // Debug optionnel pour confirmer dans la console
                //GD.Print($"Icon updated for {cardModel.Id.Entry} to {elementPool.BigEnergyIconPath}");
            }
        }
    }
}