using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Powers;

public class EarthSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => this.GetInternalData<Data>().TempThornsCount != 0 ? PowerStackType.Counter : PowerStackType.None;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<ThornsPower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new IntVar("DisplayAmount",0), //could not find how to access DisplayAmount in localization otherwise
    ]);
    
    
    public override int DisplayAmount => this.GetInternalData<Data>().TempThornsCount;
    
    protected override object InitInternalData() => new Data();
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        
        //Only trigger if the owner of this power play a card
        if (Owner != cardPlay.Card.Owner.Creature) 
            return;
        

        var elementCard = cardPlay.Card as FiveElementsCard;
        // Check if the played card is a Earth element card, or if it's a neutral card with spirits form, or if it's an other mod card with spirits form
        if (elementCard != null && elementCard.IsEarth() ||
            elementCard != null && elementCard.IsNeutral() && HasSpiritsForm ||
            elementCard == null && HasSpiritsForm)
        {
            if (elementCard is EarthSpirit) //si c'est la carte qui donne le pouvoir on ne la compte pas
            {
                Flash();
                data.TempThornsCount += Amount - 1;
                DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
                await PowerCmd.Apply<ThornsPower>(Owner, Amount - 1, Owner,null);
                InvokeDisplayAmountChanged();
            }
            else
            {
                Flash();
                data.TempThornsCount += Amount;
                DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
                await PowerCmd.Apply<ThornsPower>(Owner, Amount, Owner,null);
                InvokeDisplayAmountChanged();
            }
            
        }
    }
    
    //we remove temp thorn at next turn start
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)return;
        Flash();
        //remove of the thorns given by this power
        await PowerCmd.Apply<ThornsPower>(Owner, -GetInternalData<Data>().TempThornsCount, Owner, null);
        GetInternalData<Data>().TempThornsCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
    }
    
    
    private class Data
    {
        public int TempThornsCount;
   }
}