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

    //added decrement ???
    
    
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // On ne réagit que si c'est le porteur du pouvoir Wave qui change
        // OU si c'est le pouvoir Tsunami qui est ajouté/modifié
        if (applier != Owner) return;

        if (power is WavePower || power is WaterTsunamiPower)
        {
            await SyncWaveTarget();
        }
    }

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        await base.AfterCreatureAddedToCombat(creature);
        await SyncWaveTarget();
    }

    public override async Task AfterDeath(PlayerChoiceContext context, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        await base.AfterDeath(context, creature, wasRemovalPrevented, deathAnimLength);
        // On attend un tout petit peu que la liste se mette à jour
        await SyncWaveTarget();
    }

    
    private async Task SyncWaveTarget()
    {
        if (Amount <= 0) return;

        if (HasWaterTsunami)
        {
            // --- MODE TSUNAMI : Tout le monde doit avoir la marque ---
            foreach (var enemy in CombatState.HittableEnemies)
            {
                // On vérifie le montant actuel sur l'ennemi
                var p = enemy.GetPower<WaveTargetPower>();
                if (p == null || p.Amount != Amount)
                {
                    // On applique/met à jour pour que l'ennemi ait exactement le montant du joueur
                    // PowerCmd.Apply avec un montant spécifique recalcule le total
                    decimal diff = Amount - (p?.Amount ?? 0);
                    if (diff != 0)
                    {
                        await PowerCmd.Apply<WaveTargetPower>(enemy, diff, Owner, null, true);
                    }
                }
            }
        }
        else
        {
            // --- MODE NORMAL : Uniquement le premier
            Creature? priorityTarget = CombatState.HittableEnemies.FirstOrDefault();
            if (priorityTarget == null) return;

            // On cherche qui a la marque et on nettoie les autres (au cas où on vient de perdre Tsunami)
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy == priorityTarget)
                {
                    var p = enemy.GetPower<WaveTargetPower>();
                    if (p == null || p.Amount != Amount)
                    {
                        decimal diff = Amount - (p?.Amount ?? 0);
                        await PowerCmd.Apply<WaveTargetPower>(enemy, diff, Owner, null, true);
                    }
                }
                else if (enemy.HasPower<WaveTargetPower>())
                {
                    // Si ce n'est pas la cible prio mais qu'il a encore le pouvoir, on l'enlève
                    await PowerCmd.Remove<WaveTargetPower>(enemy);
                }
            }
        }
    }
    

    
    public override async Task AfterPlayerTurnStart(
         PlayerChoiceContext choiceContext, 
         Player player)
     {
         Creature? target = CombatState.HittableEnemies.FirstOrDefault();
         await TriggerWave(CombatState, target, choiceContext);
     }

    /*
    // this could make it trigger before doom but it's less fitting thematicaly
    public override async Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // On ne veut déclencher l'effet que si c'est la fin du tour de l'enemie
        if (side == CombatSide.Enemy)
        {
            Creature? target = CombatState.HittableEnemies.FirstOrDefault();
            await TriggerWave(CombatState, target, choiceContext);
        }

        await base.BeforeTurnEndVeryEarly(choiceContext, side);
    }
*/
    public async Task TriggerWave(CombatState combatState, Creature? target, PlayerChoiceContext choiceContext)
     {
         if (target != null)
         {
             Flash();
             if (HasWaterTsunami)
             {
                 foreach (Creature t in combatState.HittableEnemies)
                 {
                     await CreatureCmd.Damage(choiceContext, t, Amount, ValueProp.Move | ValueProp.Unpowered, null, null);
                     await PowerCmd.Apply<WaveTargetPower>(t, -1, Owner, null, true);
                 }
             }
             else
             {
                 await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Move | ValueProp.Unpowered, null, null);
                 await PowerCmd.Apply<WaveTargetPower>(target, -1, Owner, null, true);
             }
         } 
         await PowerCmd.Decrement(this);
     }
     
    
    
    private bool HasWaterTsunami => this.IsMutable && this.Owner.HasPower<WaterTsunamiPower>();
}