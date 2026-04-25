using FiveElements.FiveElementsCode.Cards;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace FiveElements.FiveElementsCode.Patches;

public static class CardPileScreenPatchs
{
    [HarmonyPatch(typeof(NCardPileScreen), "_Ready")]
    public static class CardPileScreenReadyPatch
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
                if (model is NeutralCard neutralModel)
                {
                    // On cherche le Node de la carte
                    var getCardNodeMethod = grid.GetType().GetMethod("GetCardNode");
                    var cardNode = getCardNodeMethod?.Invoke(grid, new object[] { neutralModel }) as CanvasItem;

                    if (cardNode != null)
                    {
                        var frame = cardNode.GetNodeOrNull<CanvasItem>("CardContainer/Frame");
                        if (frame != null && neutralModel.CreateCustomFrameMaterial is ShaderMaterial newMat)
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