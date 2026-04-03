using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;

namespace FiveElements.FiveElementsCode.Interfaces;

public interface IOnElementStateChanged
{
    // On passe le tag pour savoir quel élément a bougé
    // On passe le booléen pour savoir s'il est devenu actif ou inactif
    Task OnElementStateChanged(CardElementTag element, bool isActive);
}