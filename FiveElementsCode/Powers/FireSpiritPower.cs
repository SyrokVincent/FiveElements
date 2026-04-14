using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Powers;

public class FireSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);
    
    
    protected override object InitInternalData() => new Data();
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        
        // Si le pouvoir vient d'être ajouté, on ignore la toute première carte jouée 
        // (qui est forcément celle qui a créé ce pouvoir)
        if (data.JustAdded)
        {
            data.JustAdded = false;
            return;
        }
        //Only trigger if the owner of this power play a card
        if (Owner != cardPlay.Card.Owner.Creature) 
            return;
        
        var elementCard = cardPlay.Card as FiveElementsCard;
        // Check if the played card is a fire element card, or if it's a neutral card with spirits form, or if it's an other mod card with spirits form
        if (elementCard != null && elementCard.IsFire() ||
            elementCard != null && elementCard.IsNeutral() && HasSpiritsForm ||
            elementCard == null && HasSpiritsForm)
        {
            Flash();
            foreach (var hittableEnemy in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<BurnPower>(hittableEnemy, Amount, Owner,null);
            }
            
        }
    }

    private class Data
    {
        public bool JustAdded = true; // to not trigger the first time you play the card giving the power
    }
}