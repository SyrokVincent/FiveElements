using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class MetalCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : FiveElementsCard(cost, type, rarity, target)
{
    protected override HashSet<CardElementTag> CanonicalElementTags => [CardElementTag.Metal];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("earth_s",IsElementActive(CardElementTag.Earth) ? EarthColor : ""),
        new StringVar("earth_e",IsElementActive(CardElementTag.Earth) ? "[/color]" : ""),
        new StringVar("metal_s",IsElementActive(CardElementTag.Metal) ? MetalColor : ""),
        new StringVar("metal_e",IsElementActive(CardElementTag.Metal) ? "[/color]" : ""),
    ];
}