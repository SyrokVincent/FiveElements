using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Interfaces;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class EarthCard : FiveElementsCard, IOnEarthStateChanged
{
    public override Material? CreateCustomFrameMaterial => EarthShader;
    
    protected EarthCard(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target)
    {
        CanonicalElementTags = [CardElementTag.Earth];
    }  
    public async Task OnEarthStateChanged(bool isActive)
    {
        DynamicVars["isEarthOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isEarthOn"),
    ]);
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    //need to overide for card that don't have Element:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Earth),
    ]);
    
}