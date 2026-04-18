using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public class WoodRoots() : WoodCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self), IOnWaterStateChanged
{
    

    protected override bool ShouldGlowGoldInternal => 
        CombatState != null && 
        (CardElementTag.Water.IsActive(CombatState) || CardElementTag.Wood.IsActive(CombatState));

    //Water:(for every 5 wave, gain 1 temp strength),
    //Wood:(for every 3 strength gain 1 strength)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isWaterOn"),
        new IntVar("WaveDivider",5),
        new IntVar("StrengthDivider",3),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        //CardKeyword.Ethereal,
        //FiveElementsKeywords.Shift,
        //FiveElementsKeywords.Echo,
        //FiveElementsKeywords.Generate,
        //CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Wood),
        HoverTipFactory.FromPower<WavePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;
        if (CardElementTag.Water.IsActive(CombatState))
        {
            
            // 1. On récupère le montant actuel de Wave
            var currentWave = play.Card.Owner.Creature.GetPowerAmount<WavePower>();
            var tempStrengthToGain = currentWave / DynamicVars["WaveDivider"].BaseValue;
            await CommonActions.ApplySelf<WoodRootsPower>(this, tempStrengthToGain);
            
            
        }
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            // 1. On récupère le montant actuel de strength
            var currentStrength = play.Card.Owner.Creature.GetPowerAmount<StrengthPower>();
            var strengthToGain = currentStrength / DynamicVars["StrengthDivider"].BaseValue;
            await CommonActions.ApplySelf<StrengthPower>(this, strengthToGain);
      
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["WaveDivider"].UpgradeValueBy(-1);
        DynamicVars["StrengthDivider"].UpgradeValueBy(-1);
    }

    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        if (element == CardElementTag.Water) await OnWaterStateChanged(isActive);
        if (element == CardElementTag.Wood) await OnWoodStateChanged(isActive);
    }

    public async Task OnWaterStateChanged(bool isActive)
    {
        DynamicVars["isWaterOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }
}