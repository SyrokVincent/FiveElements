using FiveElements.FiveElementsCode.Cards;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace FiveElements.FiveElementsCode.Patches;

public static class PatchCardPileScreen
{
    [HarmonyPatch(typeof(NCardPileScreen), "_Ready")]
    public static class PatchCardPileScreenReady
    {
        public static void Postfix(NCardPileScreen __instance)
        {
            // Ici, l'écran est prêt et les nodes sont (normalement) créés
            GD.Print($"[UI] Initialisation visuelle de l'écran : {__instance.Pile.Type}");
        
            // On récupère la grille via réflexion
            var gridField = typeof(NCardPileScreen).GetField("_grid", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var grid = gridField?.GetValue(__instance);

            if (grid == null) return;

            foreach (var model in __instance.Pile.Cards)
            {
                // Sécurité : Si le modèle est canonique, on ne touche à rien qui utilise 'Owner'
                if (model.IsCanonical) continue; 

                if (model is NeutralCard neutralModel)
                {
                    // On cherche le Node de la carte
                    var getCardNodeMethod = grid.GetType().GetMethod("GetCardNode");
                    var cardNode = getCardNodeMethod?.Invoke(grid, new object[] { neutralModel }) as CanvasItem;

                    if (cardNode != null)
                    {
                        // On vérifie que CreateCustomFrameMaterial ne va pas planter
                        // (Assure-toi d'avoir aussi ajouté le check IsCanonical dans NeutralCard)
                        var material = neutralModel.CreateCustomFrameMaterial;
            
                        var frame = cardNode.GetNodeOrNull<CanvasItem>("CardContainer/Frame");
                        if (frame != null && material is ShaderMaterial newMat)
                        {
                            frame.Material = newMat;
                            frame.QueueRedraw();
                        }
                    }
                }
            }
        }
    }
}