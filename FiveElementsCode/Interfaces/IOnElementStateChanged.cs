using FiveElements.FiveElementsCode.Enums;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Interfaces;

public interface IOnElementStateChanged 
{
    Task OnElementStateChanged(CardElementTag element, bool isActive, Creature creature);

    // Méthode utilitaire partagée
    protected bool IsMyOwner(Creature eventSource)
    {
        Creature? listenerOwner = this switch
        {
            CardModel card => card.Owner?.Creature,
            PowerModel power => power.Owner,
            RelicModel relic => relic.Owner?.Creature,
            _ => null
        };
        // Si pas de proprio, on accepte par défaut, sinon on compare
        return listenerOwner == null || listenerOwner == eventSource;
    }
}


public interface IOnWaterStateChanged : IOnElementStateChanged 
{
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive, Creature creature) 
        => (element == CardElementTag.Water && IsMyOwner(creature)) 
            ? OnWaterStateChanged(isActive, creature) 
            : Task.CompletedTask;

    Task OnWaterStateChanged(bool isActive, Creature creature);
}

public interface IOnWoodStateChanged : IOnElementStateChanged 
{
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive, Creature creature) 
        => (element == CardElementTag.Wood && IsMyOwner(creature)) 
            ? OnWoodStateChanged(isActive, creature) 
            : Task.CompletedTask;

    Task OnWoodStateChanged(bool isActive, Creature creature);
}

public interface IOnFireStateChanged : IOnElementStateChanged 
{
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive, Creature creature) 
        => (element == CardElementTag.Fire && IsMyOwner(creature)) 
            ? OnFireStateChanged(isActive, creature) 
            : Task.CompletedTask;

    Task OnFireStateChanged(bool isActive, Creature creature);
}

public interface IOnEarthStateChanged : IOnElementStateChanged 
{
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive, Creature creature) 
        => (element == CardElementTag.Earth && IsMyOwner(creature)) 
            ? OnEarthStateChanged(isActive, creature) 
            : Task.CompletedTask;

    Task OnEarthStateChanged(bool isActive, Creature creature);
}

public interface IOnMetalStateChanged : IOnElementStateChanged 
{
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive, Creature creature) 
        => (element == CardElementTag.Metal && IsMyOwner(creature)) 
            ? OnMetalStateChanged(isActive, creature) 
            : Task.CompletedTask;

    Task OnMetalStateChanged(bool isActive, Creature creature);
}
