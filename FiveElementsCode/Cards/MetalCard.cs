using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Interfaces;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class MetalCard : FiveElementsCard, IOnMetalStateChanged
{
    //public override Material? CreateCustomFrameMaterial => ShaderUtils.GenerateHsv(0.55f, 0.2f, 1.2f);
    public override Material? CreateCustomFrameMaterial => MetalShader;
    
    protected MetalCard(int cost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary = true, bool autoAdd = true)
        : base(cost, type, rarity, target,showInCardLibrary,autoAdd)
    {
        CanonicalElementTags = [CardElementTag.Metal];
    }
    
    public async Task OnMetalStateChanged(bool isActive, Creature creature)
    {
        if (Owner.Creature != creature) return;
        DynamicVars["isMetalOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isMetalOn"),
    ]);
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    //need to overide for card that don't have Element:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Metal),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
    ]);
    
}