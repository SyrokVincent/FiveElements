using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Powers;

public class WoodSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => this.GetInternalData<Data>().TempStrengthCount != 0 ? PowerStackType.Counter : PowerStackType.None;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new IntVar("DisplayAmount",0), //could not find how to access DisplayAmount in localization otherwise
    ]);
    
    
    public override int DisplayAmount => this.GetInternalData<Data>().TempStrengthCount;
    
    protected override object InitInternalData() => new Data();
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        
        //Only trigger if the owner of this power play a card
        if (Owner != cardPlay.Card.Owner.Creature) 
            return;
        
        var elementCard = cardPlay.Card as FiveElementsCard;
        // Check if the played card is a Wood element card, or if it's a neutral card with spirits form, or if it's an other mod card with spirits form
        if (elementCard != null && elementCard.IsWood() ||
            elementCard != null && elementCard.IsNeutral() && HasSpiritsForm ||
            elementCard == null && HasSpiritsForm)
        {
            if (elementCard is WoodSpirit) //si c'est la carte qui donne le pouvoir on ne la compte pas
            {
                Flash();
                data.TempStrengthCount += Amount - 1;
                DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
                await PowerCmd.Apply<StrengthPower>(Owner, Amount - 1, Owner,null);
                InvokeDisplayAmountChanged();
            }
            else
            {
                Flash();
                data.TempStrengthCount += Amount;
                DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
                await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner,null);
                InvokeDisplayAmountChanged();
            }
            
        }
    }
    
    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)return;
        Flash();
        //remove of the strength given by this power
        await PowerCmd.Apply<StrengthPower>(Owner, -GetInternalData<Data>().TempStrengthCount, Owner, null);
        GetInternalData<Data>().TempStrengthCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
    }
    
    private class Data
    {
        public int TempStrengthCount;
   }
}