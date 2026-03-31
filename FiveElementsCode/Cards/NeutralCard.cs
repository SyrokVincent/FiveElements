using MegaCrit.Sts2.Core.Entities.Cards;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class NeutralCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : FiveElementsCard(cost, type, rarity, target)
{
    protected override HashSet<CardElementTag> CanonicalElementTags => [CardElementTag.Neutral];

}