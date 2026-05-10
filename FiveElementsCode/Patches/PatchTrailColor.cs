using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Commands;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace FiveElements.FiveElementsCode.Patches;

public static class ShuffleVisualManager
{
    // La réserve qui garantit le bon nombre de couleurs
    public static readonly List<Color> ColorPool = new();
    public static readonly ConditionalWeakTable<NCardFlyShuffleVfx, object> AssignedColors = new();
}

[HarmonyPatch]
public static class TrailColorPatches
{
    // 1. On intercepte le DEBUT du Shuffle pour remplir notre réserve
    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Shuffle))]
    [HarmonyPrefix]
    public static void PrefixCaptureDiscardPool(Player player)
    {
        if (player == null) return;

        CardPile discard = PileType.Discard.GetPile(player);
        
        // On vide la réserve précédente
        ShuffleVisualManager.ColorPool.Clear();

        // On remplit avec EXACTEMENT ce qui est dans la défausse avant qu'elle ne soit vidée
        foreach (var card in discard.Cards)
        {
            ShuffleVisualManager.ColorPool.Add(card.GetTrailColor(card.Owner?.Creature));
        }

        // On mélange notre liste de couleurs pour que l'effet visuel soit varié
        //ShuffleVisualManager.ColorPool.Shuffle();
    }

    // 2. Chaque projectile créé par StS2 pioche dans cette réserve
    [HarmonyPatch(typeof(NCardFlyShuffleVfx), nameof(NCardFlyShuffleVfx.Create))]
    [HarmonyPostfix]
    public static void PostfixAssignFromPool(NCardFlyShuffleVfx __result)
    {
        if (__result != null && ShuffleVisualManager.ColorPool.Count > 0)
        {
            // On distribue une couleur de la réserve
            Color c = ShuffleVisualManager.ColorPool[0];
            ShuffleVisualManager.ColorPool.RemoveAt(0);
            
            ShuffleVisualManager.AssignedColors.Add(__result, c);
        }
    }

    // 3. Application au Trail lors du Ready
    [HarmonyPatch(typeof(NCardTrailVfx), "_Ready")]
    [HarmonyPostfix]
    public static void PostfixTrailReady(NCardTrailVfx __instance)
    {
        var field = typeof(NCardTrailVfx).GetField("_nodeToFollow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nodeToFollow = field?.GetValue(__instance);

        // Couleur par défaut si la réserve est vide (ce qui ne devrait pas arriver)
        Color finalColor = new Color(0.2f, 0.2f, 0.2f);

        if (nodeToFollow is NCard nCard && nCard.Model != null)
        {
            // Pioche classique
            finalColor = nCard.Model.GetTrailColor(nCard.Model.Owner?.Creature);
        }
        else if (nodeToFollow is NCardFlyShuffleVfx shuffleVfx)
        {
            // Shuffle
            if (ShuffleVisualManager.AssignedColors.TryGetValue(shuffleVfx, out var stored))
            {
                finalColor = (Color)stored;
            }
        }

        __instance.GetNodeOrNull<Node2D>("Sprites")?.SetIndexed("modulate", finalColor);
        __instance.GetNodeOrNull<Node2D>("Trails")?.SetIndexed("modulate", finalColor);
    }
}
/*
// Extension pour mélanger la liste de couleurs
public static class ColorExtensions {
    private static readonly System.Random _rng = new();
    public static void Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = _rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}*/