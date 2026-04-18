using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

public class MetalEdgePower : FiveElementsPower
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
        
        
        // On vérifie que c'est bien une attaque venant du porteur du pouvoir
        if (!props.IsPoweredAttack() || cardSource == null || cardSource.Owner.Creature != Owner)
            return 1m;

        // On applique le boost de amount%
        return 1m + Amount / 100m;
        
    }

    public override async Task AfterAttack(AttackCommand command)
    {
        // On vérifie que la source de l'attaque est bien le porteur du pouvoir
        if (command.Attacker == Owner)
        {
            await PowerCmd.Remove(this);
        }
    }

}