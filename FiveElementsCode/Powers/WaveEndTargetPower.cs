using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace FiveElements.FiveElementsCode.Powers;

  
public sealed class WaveEndTargetPower : WaveTargetPower
{
    public override async Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        await ProcessWaveEffect(CombatState);
    }
    
    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (Amount <= 0) yield break;
        
        yield return new HealthBarForecastSegment(
            amount: Amount,
            color: new Color("#1E90FF"),
            direction: HealthBarForecastDirection.FromLeft,
            order: 1 //I think there is a bug with this, whatever the number it always superpose with doom
        );
    }
}