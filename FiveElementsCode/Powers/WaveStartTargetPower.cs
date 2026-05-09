using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

  
public sealed class WaveStartTargetPower : WaveTargetPower
{

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        await ProcessWaveEffect(combatState);
    }
    
    
    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (Amount <= 0) yield break;
        
        yield return new HealthBarForecastSegment(
            amount: Amount,
            color: new Color("#1E90FF"),
            direction: HealthBarForecastDirection.FromLeft,
            order: 0 //I think there is a bug with this, whatever the number it always superpose with doom
        );
    }
    
}