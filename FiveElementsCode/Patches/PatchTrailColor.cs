using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using FiveElements.FiveElementsCode.Extensions;

namespace FiveElements.FiveElementsCode.Patches;

public static class ShuffleVisualManager
{
    public static readonly Queue<Color> PendingColors = new();
    public static readonly ConditionalWeakTable<NCardFlyShuffleVfx, object> AssignedColors = new();

    public static void ApplyManualPatches(Harmony harmony)
    {
        // On cible la méthode Add qui prend CardModel et CardPile (celle du code source Shuffle)
        var method1 = AccessTools.Method(typeof(CardPileCmd), "Add", new Type[] { typeof(CardModel), typeof(CardPile) });
        
        // On cible AUSSI la version qui prend PileType au cas où (plus sécurisé)
        var method2 = AccessTools.Method(typeof(CardPileCmd), "Add", new Type[] { typeof(CardModel), typeof(PileType), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool) });

        var prefix = AccessTools.Method(typeof(TrailColorPatches), nameof(TrailColorPatches.PrefixCardPileCmdAddUniversal));
        
        if (method1 != null) harmony.Patch(method1, prefix: new HarmonyMethod(prefix));
        if (method2 != null) harmony.Patch(method2, prefix: new HarmonyMethod(prefix));
    }
}

[HarmonyPatch]
public static class TrailColorPatches
{
    // Cette méthode va capturer la couleur peu importe la version de Add appelée
    public static void PrefixCardPileCmdAddUniversal(CardModel card)
    {
        // On simplifie : si on ajoute une carte pendant que le jeu est en état de "Shuffle", on prend sa couleur
        // On peut vérifier si le combat est en cours pour éviter les faux positifs hors combat
        if (card != null)
        {
            var owner = card.Owner?.Creature;
            Color color = card.GetTrailColor(owner);
            ShuffleVisualManager.PendingColors.Enqueue(color);
        }
    }

    [HarmonyPatch(typeof(NCardFlyShuffleVfx), nameof(NCardFlyShuffleVfx.Create))]
    [HarmonyPostfix]
    public static void PostfixShuffleVfxCreate(NCardFlyShuffleVfx __result)
    {
        if (__result != null && ShuffleVisualManager.PendingColors.Count > 0)
        {
            Color color = ShuffleVisualManager.PendingColors.Dequeue();
            ShuffleVisualManager.AssignedColors.Add(__result, color);
        }
    }

    [HarmonyPatch(typeof(NCardTrailVfx), "_Ready")]
    [HarmonyPostfix]
    public static void PostfixTrailReady(NCardTrailVfx __instance)
    {
        var field = typeof(NCardTrailVfx).GetField("_nodeToFollow", BindingFlags.NonPublic | BindingFlags.Instance);
        var nodeToFollow = field?.GetValue(__instance);

        // Si rien n'est trouvé, on reste sur le gris neutre
        Color finalColor = new Color(0.3f, 0.3f, 0.3f);

        if (nodeToFollow is NCard nCard && nCard.Model != null)
        {
            finalColor = nCard.Model.GetTrailColor(nCard.Model.Owner?.Creature);
        }
        else if (nodeToFollow is NCardFlyShuffleVfx shuffleVfx)
        {
            if (ShuffleVisualManager.AssignedColors.TryGetValue(shuffleVfx, out var stored))
            {
                finalColor = (Color)stored;
            }
            else if (ShuffleVisualManager.PendingColors.Count > 0)
            {
                // Backup : si la ConditionalWeakTable a raté le lien, on pioche directement
                finalColor = ShuffleVisualManager.PendingColors.Dequeue();
            }
        }

        // On force l'application
        var sprites = __instance.GetNodeOrNull<Node2D>("Sprites");
        if (sprites != null) sprites.Modulate = finalColor;

        var trails = __instance.GetNodeOrNull<Node2D>("Trails");
        if (trails != null) trails.Modulate = finalColor;
    }
}