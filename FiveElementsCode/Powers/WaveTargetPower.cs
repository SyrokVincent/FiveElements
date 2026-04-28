using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace FiveElements.FiveElementsCode.Powers;

  
public sealed class WaveTargetPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override bool IsVisibleInternal => false;
    
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