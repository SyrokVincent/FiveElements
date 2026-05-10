using FiveElements.FiveElementsCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

  
public sealed class WavePower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new IntVar("HasWaterTsunami", (Owner != null && HasWaterTsunami) ? 1M : 0M),
        new IntVar("HasWaterRelic", (Owner != null && HasWaterRelic) ? 1M : 0M),
    ]);
    
    // owner =! null needed, do not remove
    private bool HasWaterTsunami => 
        this.IsMutable && 
        Owner != null && 
        Owner.HasPower<WaterTsunamiPower>();

    private bool HasWaterRelic => 
        Owner != null &&
        Owner.Player.Relics != null && 
        Owner.Player.Relics.Any(r => r is WaterRelic);
    
    
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // On ne réagit que si c'est le porteur du pouvoir Wave qui change
        // OU si c'est le pouvoir Tsunami qui est ajouté/modifié
        if (applier != Owner) return;

        if (power is WavePower || power is WaterTsunamiPower)
        {
            await SyncWaveTarget(choiceContext);
        }
    }


    public override async Task AfterDeath(PlayerChoiceContext context, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        await base.AfterDeath(context, creature, wasRemovalPrevented, deathAnimLength);
        
        await SyncWaveTarget(context);
    }
    
    //needed for surrounded power
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            await SyncWaveTarget(context);
        }
    }

    //needed for surrounded power
    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (potion.Owner.Creature == Owner)
        {
            await SyncWaveTarget(new ThrowingPlayerChoiceContext());
        }
    }
    
    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        await base.AfterCreatureAddedToCombat(creature);
        await SyncWaveTarget(new ThrowingPlayerChoiceContext());
    }

    //needed for enemies that revive with the IllusionPower
    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // On ne synchronise que si c'est un soin (delta > 0) 
        // ET que la créature a le potentiel de ressusciter (IllusionPower)
        if (delta > 0 && creature.HasPower<IllusionPower>())
        {
            await SyncWaveTarget(new ThrowingPlayerChoiceContext());
        }
    }
    
    private async Task SyncWaveTarget(PlayerChoiceContext context)
    {
        // Mise à jour de la localisation
        DynamicVars["HasWaterTsunami"].BaseValue = HasWaterTsunami ? 1M : 0M;
        DynamicVars["HasWaterRelic"].BaseValue = HasWaterRelic ? 1M : 0M;
        
        // On ne fait plus de return si Amount <= 0, car on doit 
        // notifier les ennemis que la marque doit passer à 0.
        decimal currentPowerAmount = Amount > 0 ? Amount : 0; 
    
        if (Owner?.Player == null) return;
        ulong myNetId = Owner.Player.NetId;

        // Déterminer quel type de pouvoir synchroniser
        // On utilise WaveEndTargetPower si on a la relique, sinon WaveStartTargetPower
        bool useEndPower = HasWaterRelic;

        foreach (var enemy in CombatState.HittableEnemies)
        {
            bool isTarget = (HasWaterTsunami || enemy == GetPriorityTarget());
            decimal targetAmount = (currentPowerAmount > 0 && isTarget) ? currentPowerAmount : 0;

            if (useEndPower)
            {
                await UpdateShare<WaveEndTargetPower>(context, enemy, targetAmount, myNetId);
                // Sécurité : Si on vient de ramasser la relique, il faut nettoyer l'autre marque
                await UpdateShare<WaveStartTargetPower>(context, enemy, 0, myNetId);
            }
            else
            {
                await UpdateShare<WaveStartTargetPower>(context, enemy, targetAmount, myNetId);
                // Sécurité : Si on a perdu la relique, on nettoie la marque de fin de tour
                await UpdateShare<WaveEndTargetPower>(context, enemy, 0, myNetId);
            }
        }
    }
    
    private async Task UpdateShare<T>(PlayerChoiceContext context, Creature target, decimal targetAmount, ulong myNetId) where T : PowerModel
    {
        // On récupère le pouvoir existant
        var p = target.GetPower<T>();
    
        // On récupère la part actuelle (nécessite un cast car GetData n'est pas dans PowerModel)
        decimal currentShare = 0;
        if (p is WaveStartTargetPower wt) currentShare = wt.GetData().PlayerMarks.GetValueOrDefault(myNetId, 0M);
        if (p is WaveEndTargetPower wet) currentShare = wet.GetData().PlayerMarks.GetValueOrDefault(myNetId, 0M);

        decimal diff = targetAmount - currentShare;
        if (diff != 0)
        {
            await PowerCmd.Apply<T>(context, target, diff, Owner, null, true);
        
            var updated = target.GetPower<T>();
            if (updated != null)
            {
                if (updated is WaveStartTargetPower uwt) uwt.SetPlayerShare(myNetId, targetAmount);
                if (updated is WaveEndTargetPower uwet) uwet.SetPlayerShare(myNetId, targetAmount);

                if (updated.Amount <= 0) await PowerCmd.Remove(updated);
            }
        }
    }
    
    private Creature? GetPriorityTarget()
    {
        var surrounded = Owner.GetPower<SurroundedPower>();

        if (surrounded != null)
        {
            if (surrounded.Facing == SurroundedPower.Direction.Left)
            {
                // On prend le dernier (le plus proche de nous à gauche)
                return CombatState.HittableEnemies
                    .LastOrDefault(e => e.HasPower<BackAttackLeftPower>());
            }
            else
            {
                // On prend le premier (le plus proche de nous à droite)
                return CombatState.HittableEnemies
                    .FirstOrDefault(e => e.HasPower<BackAttackRightPower>());
            }
        }
            
        // Si pas de Surrounded ou cible non trouvée, premier par défaut
        return CombatState.HittableEnemies.FirstOrDefault();
    }
    
    //now only used by card that trigger wave,
    public async Task TriggerWave(ICombatState combatState, PlayerChoiceContext choiceContext, Creature? target = null)
    {
        // 1. Déterminer la cible
        if (target == null && !HasWaterTsunami) target = GetPriorityTarget();

        Flash();
    
        // 2. Infliger les dégâts
        if (HasWaterTsunami)
        {
            foreach (Creature t in combatState.HittableEnemies)
            {
                await CreatureCmd.Damage(choiceContext, t, Amount, ValueProp.Move | ValueProp.Unpowered, null, null);
            }
        }
        else if (target != null)
        {
            await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Move | ValueProp.Unpowered, null, null);
        }

        // 3. Réduire le buff du joueur et Synchroniser ses wave (Nettoie les morts, transfère les marques, ajuste les montants)
        await PowerCmd.Decrement(this);
        await SyncWaveTarget(choiceContext);
    }
}