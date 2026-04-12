using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class MetalMettlePower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new IntVar("DisplayAmount",0), //could not find how to acces DisplayAmount in localization otherwise
    ]);
    
    
    private int _timeTriggeredThisTurn = 0;
    private int _vigorConsumedByCurrentCard = 0;
    private bool _isTrackingConsumption = false;

    public override int DisplayAmount => Amount - _timeTriggeredThisTurn;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _timeTriggeredThisTurn = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
        
        return Task.CompletedTask;
    }

    public override async Task BeforeCardPlayed( CardPlay play)
    {
        if (play.Card.Owner != Owner.Player) return;
        
        //todo remove cadrtypeattack if other source of vigor consumption
        if (play.Card.Type == CardType.Attack && _timeTriggeredThisTurn < Amount)
        {
            _isTrackingConsumption = true;
            _vigorConsumedByCurrentCard = 0;
        }
    }

    // On utilise ce hook juste pour compter les pertes, sans rien modifier
    public override Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
    {
        // Si on est en train de tracker une carte ET que c'est de la Vigueur qui baisse (< 0)
        if (_isTrackingConsumption && power is VigorPower && amount < 0 && target == Owner)
        {
            // On accumule la valeur absolue de la perte
            _vigorConsumedByCurrentCard += (int)Math.Abs(amount);
        }
        return base.BeforePowerAmountChanged(power, amount, target, applier, cardSource);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Card.Owner != Owner.Player) return;
        if (_isTrackingConsumption && _vigorConsumedByCurrentCard > 0)
        {
            _timeTriggeredThisTurn++;
            
            // UI Update
            DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
            InvokeDisplayAmountChanged();
            this.Flash();

            // On rend exactement ce qui a été consommé, peu importe si la carte a donné de la vigueur entre temps
            await PowerCmd.Apply<VigorPower>(Owner, _vigorConsumedByCurrentCard, Owner, null);
        }

        // Reset pour la prochaine carte
        _isTrackingConsumption = false;
        _vigorConsumedByCurrentCard = 0;
        
        await base.AfterCardPlayed(choiceContext, play);
    }
}