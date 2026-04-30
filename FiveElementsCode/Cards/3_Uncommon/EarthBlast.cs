using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class EarthBlast() : EarthCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    //Deal 10 Damage,
    //Earth:(Deal 1 more for every 10 block gained this fight) (damage blocked ?)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(10), // Dégâts de base
        new ExtraDamageVar(1),    // Dégâts bonus par brûlure
        new IntVar("Divider",8), // divise les degat bonus
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            var divider = card.DynamicVars["Divider"].BaseValue;
        
            var earthIsActive = card.CombatState != null && CardElementTag.Earth.IsActive(card.CombatState);

            if (!earthIsActive || divider <= 0) 
                return 0;
            
            // On renvoie le multiplicateur (nombre de fois qu'on ajoute ExtraDamageVar)
            var blockGainedThisCombat = CombatManager.Instance.History.Entries
                .OfType<BlockGainedEntry>()
                .Where(e => e.Amount > 0 && 
                            e.Receiver == card.Owner.Creature)
                .Sum(e => (int)e.Amount);
            return (decimal)(blockGainedThisCombat / divider);
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.Static(StaticHoverTip.Block),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;
        
        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2);
        DynamicVars["Divider"].UpgradeValueBy(-2);
    }
}