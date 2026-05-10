using BaseLib.Extensions;
using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Enums;
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

public sealed class EarthSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => this.GetInternalData<Data>().TempThornsCount != 0 ? PowerStackType.Counter : PowerStackType.None;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<ThornsPower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        EarthSpiritVars.EarthSpirit, //this number need to be the same as the one on firespirit
        new IntVar("DisplayAmount",0), //could not find how to access DisplayAmount in localization otherwise
    ]);
    
    
    public override int DisplayAmount => this.GetInternalData<Data>().TempThornsCount;
    
    protected override object InitInternalData() => new Data();
 
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        
        var shouldTrigger = false;

        // CAS A : C'est notre propre carte
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            if (cardPlay.Card.CountAsElement(CardElementTag.Earth, Owner)) 
                shouldTrigger = true;
        }
        // CAS B : C'est une carte alliée sous BodyAttunement
        else if (cardPlay.Card.Owner.HasPower<MindAndBodyAttunementBodyPower>() && Owner.HasPower<MindAndBodyAttunementMindPower>())
        {
            // On vérifie si NOTRE Echo actuel est Terre
            // Car si on a BodyAttunementOwner, c'est notre Echo qui "teinte" les cartes de l'allié
            if (Owner.GetElementalStatus().Echo.Contains(CardElementTag.Earth)) 
                shouldTrigger = true;
        }

        // 2. Exécution de l'effet
        if (shouldTrigger)
        {
        
            // Calcul du bonus (Gestion spécifique pour la carte EarthSpirit)
            var bonus = Amount;
            if (cardPlay.Card is EarthSpirit && cardPlay.Card.Owner.Creature == Owner) 
                bonus -= DynamicVars["EarthSpiritPower"].IntValue; 

            if (bonus > 0)
            {
                Flash();
                data.TempThornsCount += bonus;
                DynamicVars["DisplayAmount"].BaseValue = data.TempThornsCount;
                await PowerCmd.Apply<ThornsPower>(context, Owner, bonus, Owner, null);
                InvokeDisplayAmountChanged();
            }
        }
    }
    
    
    //we remove temp thorn at next turn start
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player ||  GetInternalData<Data>().TempThornsCount == 0)return;
        Flash();
        //remove of the thorns given by this power
        await PowerCmd.Apply<ThornsPower>(choiceContext,Owner, -GetInternalData<Data>().TempThornsCount, Owner, null);
        GetInternalData<Data>().TempThornsCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
    }
    
    
    private class Data
    {
        public int TempThornsCount;
   }
}