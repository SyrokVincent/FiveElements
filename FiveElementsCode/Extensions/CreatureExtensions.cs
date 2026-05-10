using System.Runtime.CompilerServices;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace FiveElements.FiveElementsCode.Extensions;

public static class CreatureExtensions
{
    // On lie l'objet Element à l'instance de la Creature.
    // Quand la Creature est détruite, l'Element est supprimé de la mémoire.
    private static readonly ConditionalWeakTable<Creature, Element> StatusMap = new();

    public static Element GetElementalStatus(this Creature creature)
    {
        // On récupère ou on crée l'élément spécifique à cette créature
        return StatusMap.GetValue(creature, c => new Element(c));
    }
    
    public static int GetEchoStateForDescription(this Creature creature)
    {
        var status = creature.GetElementalStatus();
    
        // Si l'Echo contient tous les éléments (Neutre + les 5)
        if (status.Echo.Count >= 6) return 6; 

        // Retourne le dernier élément ajouté (ou Neutre/0 si vide)
        return (int)status.Echo.LastOrDefault();
    }
    
    
    public static bool IsAnyElementActive(this Creature creature)
    {
        return CardElementTag.Water.IsActive(creature) ||
               CardElementTag.Wood.IsActive(creature)  ||
               CardElementTag.Fire.IsActive(creature)  ||
               CardElementTag.Earth.IsActive(creature) ||
               CardElementTag.Metal.IsActive(creature);
    }
}