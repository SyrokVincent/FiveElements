using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace FiveElements.FiveElementsCode.Powers;

public class WaterCanonPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WavePower>(),
    ];
    
    //late to trigger after wave trigger
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<WavePower>(Owner, -Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
    
}