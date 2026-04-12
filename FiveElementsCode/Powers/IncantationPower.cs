using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace FiveElements.FiveElementsCode.Powers;

public class IncantationPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Fulu>(),
    ];
    // public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    // {
    // }

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        CombatState combatState)
    {
        if (player != Owner.Player) return;
        Flash();
        await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner.Player, Amount,false, combatState);
    }
    
}