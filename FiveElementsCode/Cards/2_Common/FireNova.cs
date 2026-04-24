using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class FireNova() : FireCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AllEnemies)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(CombatState);

    //Deal 3 Heat damage to all enemies, Fire:(apply 3 burn to all enemies)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(3,ValueProp.Move),
        new PowerVar<BurnPower>(3),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Heat),
        HoverTipFactory.FromPower<BurnPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        foreach (var hittableEnemy in CombatState.HittableEnemies)
        {
            await DealHeatDamage(choiceContext, hittableEnemy, DynamicVars.Damage);
        }
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<BurnPower>(choiceContext, targets, this.DynamicVars["BurnPower"].BaseValue, this.Owner.Creature, this);
        }
    }
    

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["BurnPower"].UpgradeValueBy(1);
    }
}