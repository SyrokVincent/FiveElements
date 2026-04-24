using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Powers;

public class MetalCorePower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new IntVar("XBonus", (int)Math.Pow(2, (double)Amount)),
    ]);

    public override int DisplayAmount => DynamicVars["XBonus"].IntValue;

    /*
    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        // On ne déclenche l'effet que si c'est le tour du Joueur
        if (side != CombatSide.Player)
            return;
        
        // Calcul de la puissance : 2^Amount
        // Amount 1 = 2
        // Amount 2 = 4
        // Amount 3 = 8
        // Amount 4 = 16
        decimal multiplier = (decimal)Math.Pow(2, (double)Amount);
        
        // On applique Double Damage égal au nombre de charges (Amount)
        await PowerCmd.Apply<MetalCoreDoublePower>(Owner, multiplier, Owner,null);

        // On retire ce pouvoir après utilisation
        await PowerCmd.Remove(this);
    }*/

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // On ne déclenche l'effet que si c'est le tour du Joueur
        if (player != Owner.Player)
            return;
        
        // Calcul de la puissance : 2^Amount
        // Amount 1 = 2
        // Amount 2 = 4
        // Amount 3 = 8
        // Amount 4 = 16
        decimal multiplier = (decimal)Math.Pow(2, (double)Amount);
        
        // On applique Double Damage égal au nombre de charges (Amount)
        await PowerCmd.Apply<MetalCoreDoublePower>(choiceContext, Owner, multiplier, Owner,null);

        // On retire ce pouvoir après utilisation
        await PowerCmd.Remove(this);
    }
}