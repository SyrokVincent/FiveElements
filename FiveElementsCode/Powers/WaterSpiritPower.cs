using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Powers;

public class WaterSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1),
        new IntVar("DisplayAmount",0), //could not find how to acces DisplayAmount in localization otherwise
    ]);
    
    public override int DisplayAmount => Amount - this.GetInternalData<Data>().TriggerCount;
    
    protected override object InitInternalData() => new Data();
    
    //faut mettre ça a true pour que les power ne stack pas ????
    //public override bool IsInstanced => true;
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        
        // Si le pouvoir vient d'être ajouté, on ignore la toute première carte jouée 
        // (qui est forcément celle qui a créé ce pouvoir)
        if (data.JustAdded)
        {
            data.JustAdded = false;
            DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
            InvokeDisplayAmountChanged();
            return;
        }
        //Only trigger if the owner of this power play a card
        if (Owner != cardPlay.Card.Owner.Creature) 
            return;

        //Check if we have remaining triggers
        if (data.TriggerCount >= Amount) 
            return;
        
        // Check if the played card is a Water element card, or if it's a neutral card with spirits form, or if it's an other mod card with spirits form
        if (cardPlay.Card.CountsAsElement(CardElementTag.Water,Owner))
        {
            Flash();
            data.TriggerCount++;
            DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
            InvokeDisplayAmountChanged();
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner.Player);
        }
    }
    
    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)return;
        Flash();
        GetInternalData<Data>().TriggerCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
        //Amount = AmountOnTurnStart;
    }

    private class Data
    {
        public int TriggerCount;
        public bool JustAdded = true; // to not trigger the first time you play the card giving the power
    }
}