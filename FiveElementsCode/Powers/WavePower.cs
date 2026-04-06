using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards.Rare;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

  
public class WavePower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    // {
    // }

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        CombatState combatState)
    {
            Creature target = combatState.HittableEnemies.First();
            await TriggerWave(combatState, target, choiceContext);
    }

    public async Task TriggerWave(CombatState combatState, Creature target, PlayerChoiceContext choiceContext)
    {
        Flash();
        if (HasWaterTsunami)
        {
            IReadOnlyList<Creature> targets = combatState.HittableEnemies;
            foreach (Creature t in targets)
            {
                await CreatureCmd.Damage( choiceContext,t, Amount,ValueProp.Move | ValueProp.Unpowered,null,null);
            }
        }
        else
        {
            await CreatureCmd.Damage( choiceContext,target, Amount,ValueProp.Move | ValueProp.Unpowered,null,null);
        }
        
    }
    
    private bool HasWaterTsunami
    {
        get => this.IsMutable && this.Owner.HasPower<WaterTsunamiPower>();
    }
}