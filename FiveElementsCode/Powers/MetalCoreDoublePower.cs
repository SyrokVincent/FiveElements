using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

public class MetalCoreDoublePower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);

    
    public override decimal ModifyDamageMultiplicative(
        Creature? target, 
        decimal amount, 
        ValueProp props, 
        Creature? dealer, 
        CardModel? cardSource)
    {
        // Si c'est une attaque du joueur, on multiplie par l'Amount (2, 4, 8...)
        if (cardSource != null && cardSource.Owner.Creature == Owner && props.IsPoweredAttack())
        {
            return Amount; 
        }
        return 1m;
    }

    public override async Task AfterAttack(AttackCommand command)
    {
        // On vérifie que la source de l'attaque est bien le porteur du pouvoir
        if (command.Attacker == Owner)
        {
            await PowerCmd.Remove(this);
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;
        await PowerCmd.Remove(this);
    }
}