using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class WaterCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : FiveElementsCard(cost, type, rarity, target)
{
    protected override HashSet<CardElementTag> CanonicalElementTags => [CardElementTag.Water];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("metal_s",IsElementActive(CardElementTag.Metal) ? MetalColor : ""),
        new StringVar("metal_e",IsElementActive(CardElementTag.Metal) ? "[/color]" : ""),
        new StringVar("water_s",IsElementActive(CardElementTag.Water) ? WaterColor : "" ),
        new StringVar("water_e",IsElementActive(CardElementTag.Water) ? "[/color]" : ""),
    ];
}