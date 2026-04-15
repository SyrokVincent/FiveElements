using BaseLib.Utils;
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
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Powers;

public class WoodQueenPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1),
        new PowerVar<StrengthPower>(1),
        new IntVar("DisplayAmount",Amount), //could not find how to acces DisplayAmount in localization otherwise
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
        if (card.CountsAsElement(CardElementTag.Wood,Owner))
        {
            Flash();
            data.TriggerCount++;
            DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
            InvokeDisplayAmountChanged();
            if (Owner.Player != null)
            {
                await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner.Player);
                await PowerCmd.Apply<StrengthPower>(Owner, DynamicVars["StrengthPower"].BaseValue, Owner, null);
            }
        }
    }
    
    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)return;
        Flash();
        //remove of the strength given by this power
        await PowerCmd.Apply<StrengthPower>(Owner, -GetInternalData<Data>().TriggerCount, Owner, null);
        GetInternalData<Data>().TriggerCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
    }

    private class Data
    {
        public int TriggerCount;
    }
}