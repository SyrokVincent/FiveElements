using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

  
public class WavePower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new IntVar("HasWaterTsunami", (Owner != null && HasWaterTsunami) ? 1M : 0M),
        new IntVar("HasWaterRelic", (Owner != null && HasWaterRelic) ? 1M : 0M),
    ]);
    
    //todo this need to change for multi player, it break if multiple source of wave..
    
    //added decrement ???
    
    // owner =! null needed do not remove
    private bool HasWaterTsunami => 
        this.IsMutable && 
        Owner != null && 
        Owner.HasPower<WaterTsunamiPower>();

    private bool HasWaterRelic => 
        Owner != null &&
        Owner.Player.Relics != null && 
        Owner.Player.Relics.Any(r => r is WaterRelic);
    
 
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!HasWaterRelic)
        {
            await TriggerWave(CombatState,choiceContext);
        }
    }
    
    

    public override async Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player && HasWaterRelic)
        {
            await TriggerWave(CombatState,choiceContext);
        }

        await base.BeforeTurnEndVeryEarly(choiceContext, side);
    }
    
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
        
        await SyncWaveTarget();
    }
    
    //necessaire pour prendre en compte surrounded power
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // On ne synchronise que si c'est le porteur du pouvoir qui a joué la carte
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            await SyncWaveTarget();
        }
    }

    //necessaire pour prendre en compte surrounded power
    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        // On ne synchronise que si c'est le porteur qui a utilisé la potion
        if (potion.Owner.Creature == Owner)
        {
            await SyncWaveTarget();
        }
    }
    
    private async Task SyncWaveTarget()
    {
        
        // --- MISE À JOUR DE LA LOCALISATION ---
        DynamicVars["HasWaterTsunami"].BaseValue = HasWaterTsunami ? 1M : 0M;
        DynamicVars["HasWaterRelic"].BaseValue = HasWaterRelic ? 1M : 0M;
        GD.Print("HasWaterTsunami",HasWaterTsunami,"HasWaterRelic",HasWaterRelic);
        
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
            // --- MODE NORMAL (avec gestion Surrounded) ---
            Creature? priorityTarget = null;
            var surrounded = Owner.GetPower<SurroundedPower>();

            if (surrounded != null)
            {
                if (surrounded.Facing == SurroundedPower.Direction.Left)
                {
                    // On prend le dernier (le plus proche de nous à gauche)
                    priorityTarget = CombatState.HittableEnemies
                        .LastOrDefault(e => e.HasPower<BackAttackLeftPower>());
                }
                else
                {
                    // On prend le premier (le plus proche de nous à droite)
                    priorityTarget = CombatState.HittableEnemies
                        .FirstOrDefault(e => e.HasPower<BackAttackRightPower>());
                }
            }
            
            // Si pas de Surrounded ou cible non trouvée, premier par défaut
            priorityTarget ??= CombatState.HittableEnemies.FirstOrDefault();

            if (priorityTarget == null) return;

            // On nettoie les marques sur les autres et on met à jour la cible prioritaire
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
                    // Enlève la marque si l'ennemi n'est plus "devant" le joueur
                    await PowerCmd.Remove<WaveTargetPower>(enemy);
                }
            }
        }
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
    
    public async Task TriggerWave(CombatState combatState, PlayerChoiceContext choiceContext, Creature? target = null)
     {
     
         // 1. Détermination de la cible "devant" le joueur
         if (target == null && !HasWaterTsunami)
         {
             // On récupère le pouvoir Surrounded s'il existe
             var surrounded = Owner.GetPower<SurroundedPower>();
             
             if (surrounded != null)
             {
                 if (surrounded.Facing == SurroundedPower.Direction.Left)
                 {
                     // À gauche, l'ennemi "devant" toi est le dernier de la file
                     target = combatState.HittableEnemies
                         .LastOrDefault(e => e.HasPower<BackAttackLeftPower>());
                 }
                 else
                 {
                     // À droite, l'ennemi "devant" toi est le premier de la file
                     target = combatState.HittableEnemies
                         .FirstOrDefault(e => e.HasPower<BackAttackRightPower>());
                 }
             }
             
             
             // Si pas de Surrounded ou si la cible n'a pas été trouvée, on prend le premier par défaut
             target ??= combatState.HittableEnemies.FirstOrDefault();
         }
         
         
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
             if (target == null) target = CombatState.HittableEnemies.FirstOrDefault();
             
             if (target != null)
             {
                 await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Move | ValueProp.Unpowered, null,
                     null);
                 await PowerCmd.Apply<WaveTargetPower>(target, -1, Owner, null, true);
             }
         }
         
         await PowerCmd.Decrement(this);
     }
     
    
}