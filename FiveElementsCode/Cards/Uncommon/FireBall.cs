using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Uncommon;

public sealed class FireBall() : FireCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(CombatState);

    //Deal 5 Heat damage, Fire:(deal 1 more for every 3 burn on the enemy)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(5), // Dégâts de base
        new ExtraDamageVar(1),    // Dégâts bonus par brûlure
        new IntVar("BurnDivider",3), // divise les degat bonus par burn
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            // On récupère la valeur actuelle du diviseur (2 ou 3)
            var divider = card.DynamicVars["BurnDivider"].BaseValue;
        
            var fireIsActive = card.CombatState != null && CardElementTag.Fire.IsActive(card.CombatState);

            if (!fireIsActive || target == null || divider <= 0) 
                return 0;
            // On renvoie le multiplicateur (nombre de fois qu'on ajoute ExtraDamageVar)
            var burnAmount = target.GetPowerAmount<BurnPower>();
            return (decimal)(burnAmount / divider);
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Heat),
        HoverTipFactory.FromPower<BurnPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        await DealHeatDamage(choiceContext, play.Target, DynamicVars.CalculatedDamage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(1);
        DynamicVars["BurnDivider"].UpgradeValueBy(-1);
    }
}