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

namespace FiveElements.FiveElementsCode.Powers;

public sealed class WaterSpiritPower : FiveElementsPower
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

        // 1. Déterminer si la carte doit déclencher l'effet
        bool shouldTrigger = false;

        // CAS A : C'est notre propre carte
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            if (cardPlay.Card.CountAsElement(CardElementTag.Water, Owner))
                shouldTrigger = true;
        }
        // CAS B : C'est une carte alliée sous BodyAttunement
        else if (cardPlay.Card.Owner.HasPower<MindAndBodyAttunementBodyPower>() && Owner.HasPower<MindAndBodyAttunementMindPower>())
        {
            // On vérifie NOTRE Echo
            if (Owner.GetElementalStatus().Echo.Contains(CardElementTag.Water))
                shouldTrigger = true;
        }

        // 2. Exécution si validé ET qu'il reste des utilisations
        if (shouldTrigger && data.TriggerCount < Amount)
        {
            // Logique spécifique pour ne pas consommer la dernière charge si on joue WaterSpirit soi-même
            bool isSourceCard = cardPlay.Card is WaterSpirit && cardPlay.Card.Owner.Creature == Owner;
        
            if (isSourceCard && data.TriggerCount == Amount - 1)
            {
                // On flash pour montrer que l'élément est reconnu, mais on ne gagne pas d'énergie
                Flash();
                DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
                InvokeDisplayAmountChanged();
            }
            else
            {
                Flash();
                data.TriggerCount++;
                DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
                InvokeDisplayAmountChanged();
            
                if (Owner.Player != null) 
                    await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner.Player);
            }
        }
    }
    
    public override Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side || GetInternalData<Data>().TriggerCount == 0) return Task.CompletedTask;
        Flash();
        GetInternalData<Data>().TriggerCount = 0;
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
        //Amount = AmountOnTurnStart;
    }

    private class Data
    {
        public int TriggerCount;
    }
}