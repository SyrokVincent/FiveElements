using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Powers;

public class WaterLordPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WavePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);



    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // On vérifie le propriétaire
        if (this.Applier?.Player == null || 
            cardPlay.Card.Owner != this.Applier.Player)
        {
            return Task.CompletedTask;
        }

        // On stocke le montant au moment où la carte est jouée
        this.GetInternalData<Data>().AmountsForPlayedCards[cardPlay.Card] = this.Amount;
        return Task.CompletedTask;
    }

    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
    
        // 1. On récupère la valeur stockée dans BeforeCardPlayed
        if (!data.AmountsForPlayedCards.Remove(cardPlay.Card, out var amount))
            return;

        if (cardPlay.Card.CountsAsElement(CardElementTag.Water, Owner))
        {
            this.Flash();
            await PowerCmd.Apply<WavePower>(Owner, amount, Applier, null);
        }
    }
    

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player)
            return;
        await PowerCmd.Remove(this);
    }

    protected override object InitInternalData() => new Data();
    private class Data
    {
        public readonly Dictionary<CardModel, int> AmountsForPlayedCards = new();
    }
}