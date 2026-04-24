using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Powers;

public class FireRisePower : FiveElementsPower
{
    public const int InitialThreshold = 3; //when changing base value here need to also change value in the card
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // Affiche un decompte
    
    public override int DisplayAmount 
    {
        get 
        {
            var data = GetInternalData<Data>();
            if (data == null) return InitialThreshold; // Sécurité
            //return data.Threshold - data.TimesBurned; //count down todo decide wat's the best
            return data.TimesBurned; //count up
        }
    }
    

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<FirePlume>(),
        HoverTipFactory.FromPower<BurnPower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars 
    {
        get 
        {
            var vars = base.CanonicalVars;
            var data = GetInternalData<Data>();
            int currentThreshold = data != null ? data.Threshold : InitialThreshold;
            return vars.Concat(new List<DynamicVar> 
            { 
                new IntVar("Threshold", currentThreshold) 
            });
        }
    }


    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        // On ne réagit que si c'est le porteur du pouvoir qui applique du burn (montant positif)
        if (applier != Owner || amount <= 0 || power is not BurnPower)
        {
            return;
        }

        var data = GetInternalData<Data>();
        data.TimesBurned++;

        if (data.TimesBurned >= data.Threshold)
        {
            // Reset du compteur
            data.TimesBurned = 0; 
            
            // Augmentation du seuil pour la prochaine fois
            data.Threshold += Amount; 
            DynamicVars["Threshold"].BaseValue = data.Threshold;
            InvokeDisplayAmountChanged();
            Flash();

            // Création de la plume
            if (Owner.Player != null)
                await FiveElementsCardExtensions.CreateInHand<FirePlume>(Owner.Player, Amount, false, CombatState);
        }

        InvokeDisplayAmountChanged();
    }
    

    protected override object InitInternalData() => new Data { Threshold = InitialThreshold };
    
    public int CurrentThreshold => GetInternalData<Data>().Threshold;
    internal class Data
    {
        public int TimesBurned = 0;
        public int Threshold; // Le seuil qui va augmenter
    }
}
