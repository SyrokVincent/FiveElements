using MegaCrit.Sts2.Core.Entities.Powers;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class WaterTsunamiPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}