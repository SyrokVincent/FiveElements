using System.Runtime.CompilerServices;
using FiveElements.FiveElementsCode.Models;
using MegaCrit.Sts2.Core.Combat;

namespace FiveElements.FiveElementsCode.Extensions;

public static class CombatStateExtensions
{
    // On utilise un dictionnaire pour stocker les statuts par combat
    // Cela permet de ne pas mélanger les données si plusieurs combats existent
    private static readonly ConditionalWeakTable<CombatState, Element> StatusMap = new();

    public static Element GetElement(this CombatState combatState)
    {
        // Si le statut n'existe pas encore pour ce combat, on le crée
        return StatusMap.GetValue(combatState, c => new Element(c));
    }
}