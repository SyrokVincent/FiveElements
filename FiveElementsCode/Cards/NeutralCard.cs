using FiveElements.FiveElementsCode.Enums;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class NeutralCard : FiveElementsCard
{
    protected NeutralCard(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target)
    {
        CanonicalElementTags = [CardElementTag.Neutral];
    }
}
