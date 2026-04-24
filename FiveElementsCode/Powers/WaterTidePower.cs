using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace FiveElements.FiveElementsCode.Powers;

public class WaterTidePower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WavePower>(),
    ];
    
    // public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    // {
    // }
/*
    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        CombatState combatState)
    {
        if (player != Owner.Player) return;
        Flash();
        await PowerCmd.Apply<WavePower>(Owner, Amount, Owner, null);

    }
    
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side)return;
        Flash();
        await PowerCmd.Apply<WavePower>(Owner, Amount, Owner, null);
        
    }*/

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)return;
        Flash();
        await PowerCmd.Apply<WavePower>(choiceContext, Owner, Amount, Owner, null);
    }
}