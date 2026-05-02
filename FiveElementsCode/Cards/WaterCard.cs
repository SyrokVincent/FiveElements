using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Interfaces;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class WaterCard : FiveElementsCard, IOnWaterStateChanged
{
    public override Material? CreateCustomFrameMaterial => WaterShader;
    
    protected WaterCard(int cost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary = true, bool autoAdd = true)
        : base(cost, type, rarity, target, showInCardLibrary, autoAdd)
    {
        CanonicalElementTags = [CardElementTag.Water];
    }
    
    public virtual async Task OnWaterStateChanged(bool isActive)
    {
        DynamicVars["isWaterOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isWaterOn"),
    ]);
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    //need to overide for card that don't have Element:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Water),
    ]);
    
}