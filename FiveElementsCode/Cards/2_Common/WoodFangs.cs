using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class WoodFangs() : WoodCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(Owner.Creature);

    //Deal 4x2 damage,
    //Wood:(double damage if enemy as block)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new RepeatVar(2),
        new CalculationBaseVar(4), // Base damage
        new ExtraDamageVar(1),    // bonus damage
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            var woodIsActive = card.CombatState != null && CardElementTag.Wood.IsActive(card.Owner.Creature);

            if (!woodIsActive) 
                return 0;

            if (target == null) return 0;
            var block = target.Block;
            var str = card.Owner.Creature.GetPowerAmount<StrengthPower>();
            // si block on renvoie le les degat de base + la strength (doublebling damage of the card) (c'est le nombre de fois qu'on ajoute ExtraDamageVar) 
            return block > 0 ? card.DynamicVars.CalculationBase.BaseValue + str : 0;
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null || play.Target == null) return;
        
        // Utilisation du builder d'attaque pour gérer les rebonds
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .WithHitCount(DynamicVars.Repeat.IntValue) // Nombre de répétitions
            .FromCard(this)
            .Targeting(play.Target) // Cible des ennemis au hasard à chaque coup
            .WithHitFx("vfx/vfx_attack_slash") // Effet visuel par coup
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2);
    }
}