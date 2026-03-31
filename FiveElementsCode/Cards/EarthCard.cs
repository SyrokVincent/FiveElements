using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class EarthCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : FiveElementsCard(cost, type, rarity, target)
{
    protected override HashSet<CardElementTag> CanonicalElementTags => [CardElementTag.Earth];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("fire_s",IsElementActive(CardElementTag.Fire) ? FireColor : ""),
        new StringVar("fire_e",IsElementActive(CardElementTag.Fire) ? "[/color]" : ""),
        new StringVar("earth_s",IsElementActive(CardElementTag.Earth) ? EarthColor : ""),
        new StringVar("earth_e",IsElementActive(CardElementTag.Earth) ? "[/color]" : ""),
    ];
}