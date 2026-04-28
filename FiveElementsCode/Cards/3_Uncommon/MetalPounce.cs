using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class MetalPounce() : MetalCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private decimal _extraDamageFromVigor;
    
    // Propriété pour suivre le bonus accumulé durant le combat
    private decimal ExtraDamageFromVigor
    {
        get => _extraDamageFromVigor;
        set
        {
            AssertMutable();
            _extraDamageFromVigor = value;
        }
    }
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    // old // Deal 16 damage, Metal:(vigor apply twice to this attack)
    //
    // new //Metal:(Increase this card's damage and damage this combat by your vigor (+amount)), Deal 16(+4) damage.
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(16), // Dégâts de base
        new ExtraDamageVar(1),    // Dégâts bonus par vigor
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            var metalIsActive = card.CombatState != null && CardElementTag.Metal.IsActive(card.CombatState);
            if (!metalIsActive) return 0;
            return card.Owner.Creature.GetPowerAmount<VigorPower>();
        }),
        new IntVar("DisplayVigorAmount",0),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;

        var vigorAmount = 0;
        // 2. Si l'élément Métal est actif, on augmente les dégâts permanents
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            vigorAmount = Owner.Creature.GetPowerAmount<VigorPower>();
        }
        
        //calculated damage prend deja en compte l'augmentation
        await CommonActions.CardAttack(this, play.Target,DynamicVars.CalculatedDamage).Execute(choiceContext);
        
        //on augmente ensuite pour les prochaine fois
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            if (vigorAmount > 0)
            {
                DynamicVars.CalculationBase.BaseValue += vigorAmount;
                ExtraDamageFromVigor += vigorAmount;
            }
        }
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4);
    }
    
    // Indispensable si la carte est transformée/récupérée pour garder le bonus
    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.CalculationBase.BaseValue += ExtraDamageFromVigor;
    }
    
   public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
   {
       if (power is VigorPower && power.Owner == Owner.Creature)
       {
           UpdateDisplayVigorAmount();
       }
       return Task.CompletedTask;
   }

   public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
   {
       if (card == this && card.Owner == Owner)
       {
           UpdateDisplayVigorAmount();
       }

       return Task.CompletedTask;
   }

  

   private void UpdateDisplayVigorAmount()
   {
       if (Owner?.Creature == null) return;
       DynamicVars["DisplayVigorAmount"].BaseValue = Owner.Creature.GetPowerAmount<VigorPower>();
   }
}