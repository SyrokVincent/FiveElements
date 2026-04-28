using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class WoodQueenPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SurgePower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1),
        new PowerVar<SurgePower>(2),
        new IntVar("DisplayAmount",0), //could not find how to acces DisplayAmount in localization otherwise
    ]);
    
    public override int DisplayAmount => Amount - this.GetInternalData<Data>().TriggerCount;
    
    protected override object InitInternalData() => new Data();

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        var data = GetInternalData<Data>();
        
        //Only trigger if the owner of this power draw a card
        if (Owner != card.Owner.Creature) 
            return;

        //Check if we have remaining triggers
        if (data.TriggerCount >= Amount) 
            return;
        
        // Check if the drawn card is a wood card, or if it's a neutral card with spirits form, or if it's an other mod card with spirits form
        if (card.CountAsElement(CardElementTag.Wood,Owner))
        {
            Flash();
            data.TriggerCount++;
            DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
            InvokeDisplayAmountChanged();
            if (Owner.Player != null)
            {
                await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner.Player);
                await PowerCmd.Apply<SurgePower>(choiceContext, Owner, DynamicVars["SurgePower"].BaseValue, Owner, null);
            }
        }
    }
    
    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)return;
        Flash();
        GetInternalData<Data>().TriggerCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if ( power != this) return;
        //mise a jour du displayamount
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
    }

    private class Data
    {
        public int TriggerCount;
    }
}