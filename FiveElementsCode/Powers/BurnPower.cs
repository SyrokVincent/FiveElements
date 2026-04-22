using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

public class BurnPower : FiveElementsPower
{
    
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (Amount <= 0) yield break;
        
        yield return new HealthBarForecastSegment(
            amount: Amount,
            color: new Color("#FF8C00"),
            direction: HealthBarForecastDirection.FromRight,
            order: 0
        );
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side) return;
        await CreatureCmd.Damage( new ThrowingPlayerChoiceContext(),Owner, Amount,ValueProp.Unblockable | ValueProp.Unpowered,null,null);
        if (Owner.IsAlive)
            
            if (HasFireBlossomPower)
            {
                var blossomPower = Owner.GetPower<FireBlossomPower>();
                if (blossomPower != null)
                {
                    // On passe l'instance trouvée à Decrement
                    await PowerCmd.Decrement(blossomPower);
                }
            }else await PowerCmd.Remove(this);
        else
            await Cmd.CustomScaledWait(0.1f, 0.25f);
    }
    
    private bool HasFireBlossomPower
    {
        get => this.IsMutable && this.Owner.HasPower<FireBlossomPower>();
    }
}