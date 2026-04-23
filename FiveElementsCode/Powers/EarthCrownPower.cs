namespace FiveElements.FiveElementsCode.Powers;

//no longer used
/*
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
        if (cardPlay.Card.CountAsElement(CardElementTag.Earth, Owner))
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
}*/