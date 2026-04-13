using FiveElements.FiveElementsCode.Cards;
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

public class EarthCrownPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => this.GetInternalData<Data>().TempDexterityCount != 0 ? PowerStackType.Counter : PowerStackType.None;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new IntVar("DisplayAmount",0), //could not find how to access DisplayAmount in localization otherwise
    ]);

    public override int DisplayAmount => this.GetInternalData<Data>().TempDexterityCount;

    protected override object InitInternalData() => new Data();

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // On vérifie le propriétaire
        if (this.Applier?.Player == null || 
            cardPlay.Card.Owner != this.Applier.Player)
        {
            return;
        }
        
        // 1. On vérifie si c'est une carte Earth
        if (cardPlay.Card is FiveElementsCard elementCard && elementCard.IsEarth())
        {
            var data = GetInternalData<Data>();
            
            // 2. On flashe le pouvoir pour montrer l'activation
            Flash();
            data.TempDexterityCount += this.Amount;
            DynamicVars["DisplayAmount"].BaseValue = data.TempDexterityCount;
            await PowerCmd.Apply<DexterityPower>(Owner, this.Amount, Owner, null);
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;
        Flash();
        //remove of the dex given by this power
        await PowerCmd.Apply<DexterityPower>(Owner, -GetInternalData<Data>().TempDexterityCount, Owner, null);
        GetInternalData<Data>().TempDexterityCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
        await PowerCmd.Remove(this);
    }

    private class Data
    {
        public int TempDexterityCount;
    }
}