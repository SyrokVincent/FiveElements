using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class WoodFury() : WoodCard(0,
    CardType.Attack, CardRarity.Rare,
    TargetType.RandomEnemy)
{
    protected override bool HasEnergyCostX => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //Deal 3 damage to a random enemies X times, X is doubled.
    //Wood:(X is instead tripled.)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(3, ValueProp.Move),
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
        
        if (CombatState == null) return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var xValue = ResolveEnergyXValue() * 2;
        
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            xValue = ResolveEnergyXValue() * 3;
        }
        
        
        // Utilisation du builder d'attaque pour gérer les rebonds
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(xValue) // Nombre de répétitions
            .FromCard(this)
            .TargetingRandomOpponents(CombatState)      // Cible des ennemis au hasard à chaque coup
            .WithHitFx("vfx/vfx_attack_slash")         // Effet visuel par coup
            .Execute(choiceContext);


    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}