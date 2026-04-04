using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Interfaces;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class FireCard(int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true,
    bool autoAdd = true)
    : FiveElementsCard(cost, type, rarity, target, showInCardLibrary, autoAdd), IOnElementStateChanged
{
    public override HashSet<CardElementTag> CanonicalElementTags { get; set; } = [CardElementTag.Fire];
     
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isFireOn"),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    //need to overide for card that don't have Element:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fire),
    ]);
    
    public override async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        // On ne réagit que si c'est l'élément Fire qui change d'état
        if (element == CardElementTag.Fire)
        {
            DynamicVars["isFireOn"].BaseValue = isActive ? 1 : 0;
        }

        await Task.CompletedTask;
    }
}