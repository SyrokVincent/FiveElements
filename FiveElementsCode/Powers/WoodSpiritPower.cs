using BaseLib.Extensions;
using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Enums;
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

public sealed class WoodSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => this.GetInternalData<Data>().TempStrengthCount != 0 ? PowerStackType.Counter : PowerStackType.None;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SurgePower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        WoodSpiritVars.WoodSpirit, //this number need to be the same as the one on woodspiritpower
        new IntVar("DisplayAmount",0), //could not find how to access DisplayAmount in localization otherwise
    ]);
    
    
    public override int DisplayAmount => this.GetInternalData<Data>().TempStrengthCount;
    
    protected override object InitInternalData() => new Data();
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();

        // 1. Déterminer si la carte doit déclencher l'effet
        bool shouldTrigger = false;

        // CAS A : C'est notre propre carte
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            // On vérifie si elle compte comme Bois (inclut Attune/Shift/Spirits Form)
            if (cardPlay.Card.CountAsElement(CardElementTag.Wood, Owner))
                shouldTrigger = true;
        }
        // CAS B : C'est une carte alliée sous BodyAttunement
        else if (cardPlay.Card.Owner.HasPower<MindAndBodyAttunementBodyPower>() && Owner.HasPower<MindAndBodyAttunementMindPower>())
        {
            // On vérifie NOTRE Echo actuel
            if (Owner.GetElementalStatus().Echo.Contains(CardElementTag.Wood))
                shouldTrigger = true;
        }

        // 2. Exécution si validé
        if (shouldTrigger)
        {
            // Calcul du montant de Surge
            int surgeAmount = Amount;

            // On ne réduit le montant que si c'est NOUS qui jouons la carte WoodSpirit
            if (cardPlay.Card is WoodSpirit && cardPlay.Card.Owner.Creature == Owner)
                surgeAmount -= DynamicVars["WoodSpiritPower"].IntValue;

            if (surgeAmount > 0)
            {
                Flash();
                data.TempStrengthCount += surgeAmount;
                DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
                await PowerCmd.Apply<SurgePower>(context, Owner, surgeAmount, Owner, null);
                InvokeDisplayAmountChanged();
            }
        }
    }
    
    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side ||  GetInternalData<Data>().TempStrengthCount == 0)return;
        
        Flash();
        GetInternalData<Data>().TempStrengthCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
    }
    
    private class Data
    {
        public int TempStrengthCount;
   }
}