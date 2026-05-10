using BaseLib.Cards.Variables;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class MetalRush() : MetalCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(Owner.Creature);

    // old // Return in hand and increase it's cost and it's vigor by 1 this turn, Metal:(Gain 1 vigor)
    //
    // new // Gain 2(3) vigor, Metal:(gain 1 vigor. Return in hand if it is the first card you play this turn.)
    //
    // new Deal 9(12) damage, Metal:(Increase this attack damage by 25% for each metal card played this turn)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        
        
        new CalculationBaseVar(9), 
        new ExtraDamageVar(1), 
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            var strength = card.Owner.Creature.GetPowerAmount<StrengthPower>();
            var vigor = card.Owner.Creature.GetPowerAmount<VigorPower>();
            var baseDmg = card.DynamicVars.CalculationBase.BaseValue + strength + vigor;
            
            var metalCardPlayed = ElementHistoryUtils.CountPlayedCardsOfElement(card.CombatState, card.Owner, CardElementTag.Metal);

            return baseDmg * 0.25m * metalCardPlayed;
        }),
        
        
        
        new DynamicVar("PercentDamageIncreaseBase",0),
        new DynamicVar("PercentDamageIncreaseExtra",25m),
        new CustomCalculatedVar("PercentDamageIncrease").WithMultiplier((card, target) =>
            ElementHistoryUtils.CountPlayedCardsOfElement(card.CombatState, card.Owner, CardElementTag.Metal)),
        
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    /*
    public override decimal ModifyDamageMultiplicative(
        Creature? target, 
        decimal amount, 
        ValueProp props, 
        Creature? dealer,
        CardModel? cardSource)
    {
        if (cardSource == this && props.IsPoweredAttack() && CardElementTag.Metal.IsActive(Owner.Creature))
        {
            return 1m + DynamicVars["PercentDamageIncrease"].PreviewValue/100; 
        }
        return 1m;
    }
*/
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;

        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3);
    }
}