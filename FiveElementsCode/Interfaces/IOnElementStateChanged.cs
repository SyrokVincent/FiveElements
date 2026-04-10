using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;

namespace FiveElements.FiveElementsCode.Interfaces;
/*
public interface IOnElementStateChanged
{
    // On passe le tag pour savoir quel élément a bougé
    // On passe le booléen pour savoir s'il est devenu actif ou inactif
    Task OnWaterStateChanged(bool isActive) => Task.CompletedTask;
    Task OnWoodStateChanged(bool isActive) => Task.CompletedTask;
    Task OnFireStateChanged(bool isActive) => Task.CompletedTask;
    Task OnEarthStateChanged(bool isActive) => Task.CompletedTask;
    Task OnMetalStateChanged(bool isActive) => Task.CompletedTask;
    //Task OnElementStateChanged(CardElementTag element, bool isActive);
}
*/

public interface IOnElementStateChanged 
{
    Task OnElementStateChanged(CardElementTag element, bool isActive);
}



public interface IOnWaterStateChanged : IOnElementStateChanged 
{
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive) 
        => element == CardElementTag.Water ? OnWaterStateChanged(isActive) : Task.CompletedTask;

    Task OnWaterStateChanged(bool isActive);
}
public interface IOnWoodStateChanged : IOnElementStateChanged 
{
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive) 
        => element == CardElementTag.Wood ? OnWoodStateChanged(isActive) : Task.CompletedTask;

    Task OnWoodStateChanged(bool isActive);
}
public interface IOnFireStateChanged : IOnElementStateChanged 
{
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive) 
        => element == CardElementTag.Fire ? OnFireStateChanged(isActive) : Task.CompletedTask;

    Task OnFireStateChanged(bool isActive);
}

public interface IOnEarthStateChanged : IOnElementStateChanged 
{
    // On implémente OnElementChanged par défaut pour rediriger vers la méthode spécifique
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive) 
        => element == CardElementTag.Earth ? OnEarthStateChanged(isActive) : Task.CompletedTask;

    Task OnEarthStateChanged(bool isActive);
}

public interface IOnMetalStateChanged : IOnElementStateChanged 
{
    // On implémente OnElementChanged par défaut pour rediriger vers la méthode spécifique
    Task IOnElementStateChanged.OnElementStateChanged(CardElementTag element, bool isActive) 
        => element == CardElementTag.Metal ? OnMetalStateChanged(isActive) : Task.CompletedTask;

    Task OnMetalStateChanged(bool isActive);
}