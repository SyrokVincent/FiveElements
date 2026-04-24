using System.Runtime.CompilerServices;
using FiveElements.FiveElementsCode.Models;
using MegaCrit.Sts2.Core.Combat;

namespace FiveElements.FiveElementsCode.Extensions;
/*
public static class CombatStateExtensions
{
    // On utilise un dictionnaire pour stocker les statuts par combat
    // Cela permet de ne pas mélanger les données si plusieurs combats existent
    private static readonly ConditionalWeakTable<CombatState, Element> StatusMap = new();

    public static Element GetElementalStatus(this ICombatState combatState)
    {
        // Si le statut n'existe pas encore pour ce combat, on le crée
        return StatusMap.GetValue(combatState, c => new Element(c));
    }
}

*/

public static class CombatStateExtensions
{
    // On utilise 'object' comme clé pour accepter n'importe quelle implémentation de ICombatState
    private static readonly ConditionalWeakTable<object, Element> StatusMap = new();

    public static Element GetElementalStatus(this ICombatState combatState)
    {
        // On cast vers object pour satisfaire la contrainte de classe du ConditionalWeakTable
        // GetValue s'occupe de créer l'élément s'il n'existe pas
        return StatusMap.GetValue((object)combatState, _ => new Element(combatState));
    }
}