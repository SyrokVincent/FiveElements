using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class EarthWallPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];
    
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 1. On vérifie que c'est le porteur du pouvoir qui a été touché
        // 2. On vérifie qu'il y a eu des dégâts BLOQUÉS (BlockedDamage > 0)
        // 3. On vérifie que c'est une attaque standard (PoweredAttack)
        // 4. On vérifie qu'il y a un attaquant à qui renvoyer les dégâts
        if (target != Owner || result.BlockedDamage <= 0 || !props.IsPoweredAttack() || dealer == null)
            return;

        // On renvoie exactement le montant de dégâts bloqués à l'attaquant
        // Note: On utilise ValueProp.Unpowered pour que ce renvoi ne soit pas boosté par la Force
        await CreatureCmd.Damage(choiceContext, dealer, result.BlockedDamage, ValueProp.Unpowered, Owner);
    }

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        // Au début de son tour (Joueur ou Monstre), on réduit le compteur de 1
        if (side == Owner.Side)
        {
            await PowerCmd.Decrement(this);
        }
    }
}