using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class WoodCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : FiveElementsCard(cost, type, rarity, target)
{
    protected override HashSet<CardElementTag> CanonicalElementTags => [CardElementTag.Wood];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("water_s",IsElementActive(CardElementTag.Water) ? WaterColor : "" ),
        new StringVar("water_e",IsElementActive(CardElementTag.Water) ? "[/color]" : ""),
        new StringVar("wood_s",IsElementActive(CardElementTag.Wood) ? WoodColor : ""),
        new StringVar("wood_e",IsElementActive(CardElementTag.Wood) ? "[/color]" : ""),
    ];
}