using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Interfaces;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class MetalCard(int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true,
    bool autoAdd = true)
    : FiveElementsCard(cost, type, rarity, target, showInCardLibrary, autoAdd), IOnElementStateChanged
{
    public override HashSet<CardElementTag> CanonicalElementTags { get; set; } = [CardElementTag.Metal];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isMetalOn"),
    ]);
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    //need to overide for card that don't have Element:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Metal),
    ]);

    public override async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        // On ne réagit que si c'est l'élément Metal qui change d'état
        if (element == CardElementTag.Metal)
        {
            
            DynamicVars["isMetalOn"].BaseValue = isActive ? 1 : 0;
        }

        await Task.CompletedTask;
    }
}