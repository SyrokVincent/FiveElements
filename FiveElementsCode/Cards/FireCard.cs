using FiveElements.FiveElementsCode.Enums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class FireCard(int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true,
    bool autoAdd = true)
    : FiveElementsCard(cost, type, rarity, target, showInCardLibrary, autoAdd)
{
    public override HashSet<CardElementTag> CanonicalElementTags { get; set; } = [CardElementTag.Fire];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    //need to overide for card that don't have Element:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fire),
    ]);
}