using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

  
public abstract class WaveTargetPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override bool IsVisibleInternal => true;

    protected async Task ProcessWaveEffect(ICombatState combatState)
    {
        var amountToDecrease = 0;
        var data = GetData();
        var playersToProcess = data.PlayerMarks.ToList();

        foreach (var entry in playersToProcess)
        {
            ulong playerNetId = entry.Key;
            decimal markAmount = entry.Value;

            if (markAmount <= 0) continue;

            var playerEntity = combatState.Players.FirstOrDefault(p => p.NetId == playerNetId);
            var wavePower = playerEntity?.Creature.GetPower<WavePower>();

            if (wavePower != null)
            {
                // Synchronisation des réductions
                await PowerCmd.Decrement(wavePower);
                amountToDecrease++;
                
                decimal newShare = markAmount - 1;
                SetPlayerShare(playerNetId, newShare);
            }
        }
        
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, Amount, ValueProp.Move | ValueProp.Unpowered, null, null);
        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, - amountToDecrease,  null,  null);
    }
    
    
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
    
    protected override object? InitInternalData() => new WaveData();

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (applier?.Player != null)
        {
            var data = GetInternalData<WaveData>();
            ulong netId = applier.Player.NetId; // Ton identifiant réseau
        
            // On met à jour la part du joueur spécifique
            // Amount est le montant qui vient d'être ajouté via PowerCmd.Apply
            data.PlayerMarks[netId] = data.PlayerMarks.GetValueOrDefault(netId, 0M) + (decimal)Amount;
        }
        await base.AfterApplied(applier, cardSource);
    }
    public class WaveData
    {
        // Clé : ID du joueur, Valeur : Montant de sa marque
        public Dictionary<ulong, decimal> PlayerMarks = new();
    }
    
    // Cette méthode permet de contourner le "protected" de PowerModel
    public WaveData GetData() => GetInternalData<WaveData>();

    
    public void SetPlayerShare(ulong netId, decimal amount)
    {
        var data = GetData();
        if (amount <= 0) 
            data.PlayerMarks.Remove(netId);
        else 
            data.PlayerMarks[netId] = amount;
    }
    
}