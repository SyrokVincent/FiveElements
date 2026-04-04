using FiveElements.FiveElementsCode.Enums;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class NeutralCard(int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true,
    bool autoAdd = true)
    : FiveElementsCard(cost, type, rarity, target, showInCardLibrary, autoAdd)
{
   public override HashSet<CardElementTag> CanonicalElementTags { get; set; } = [CardElementTag.Neutral];
   
   public override async Task OnElementStateChanged(CardElementTag element, bool isActive)
   {
       await Task.CompletedTask;
   }
}
