using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

public class BurnPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override Color AmountLabelColor => _normalAmountLabelColor;

    // it's used to make healthbar colored???
    public int CalculateTotalDamageNextTurn()
    {
        return Amount;
    }
    
    //todo more logic for when burn not removed?
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side) return;
        await CreatureCmd.Damage( new ThrowingPlayerChoiceContext(),Owner, Amount,ValueProp.Unblockable | ValueProp.Unpowered,null,null);
        if (Owner.IsAlive)
            await PowerCmd.Remove(this);
        else
            await Cmd.CustomScaledWait(0.1f, 0.25f);
    }
}