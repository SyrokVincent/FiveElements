using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class FireCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : FiveElementsCard(cost, type, rarity, target)
{
    protected override HashSet<CardElementTag> CanonicalElementTags => [CardElementTag.Fire];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("wood_s",IsElementActive(CardElementTag.Wood) ? WoodColor : ""),
        new StringVar("wood_e",IsElementActive(CardElementTag.Wood) ? "[/color]" : ""),
        new StringVar("fire_s",IsElementActive(CardElementTag.Fire) ? FireColor : ""),
        new StringVar("fire_e",IsElementActive(CardElementTag.Fire) ? "[/color]" : ""),
    ];
}