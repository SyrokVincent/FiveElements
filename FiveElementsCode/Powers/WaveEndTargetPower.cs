using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Powers;

  
public sealed class WaveEndTargetPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override bool IsVisibleInternal => true;
    
    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (Amount <= 0) yield break;
        
        yield return new HealthBarForecastSegment(
            amount: Amount,
            color: new Color("#1E90FF"),
            direction: HealthBarForecastDirection.FromLeft,
            order: 1 //I think there is a bug with this, whatever the number it always superpose with doom
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

    // Optionnel : Une méthode helper pour simplifier la vie de WavePower
    public void SetPlayerShare(ulong netId, decimal amount)
    {
        var data = GetData();
        data.PlayerMarks[netId] = amount;
    }
}