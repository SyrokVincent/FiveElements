using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class FireSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        FireSpiritVars.FireSpirit, //this number need to be the same as the one on firespirit
    ]);
    
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        
        //Only trigger if the owner of this power play a card
        if (Owner != cardPlay.Card.Owner.Creature) 
            return;
        
        // Check if the played card is a fire element card, or if it's a neutral card with spirits form, or if it's an other mod card with spirits form
        if (cardPlay.Card.CountAsElement(CardElementTag.Fire,Owner))
        {
            if (cardPlay.Card is FireSpirit) //si c'est la carte qui donne le pouvoir on ne la compte pas grace au -DynamicVars["FireSpiritPower"].BaseValue)
            {
                
                Flash();
                var targets = CombatState.HittableEnemies;
                await PowerCmd.Apply<BurnPower>(context, targets, Amount - DynamicVars["FireSpiritPower"].BaseValue, this.Owner, null);
            }
            else
            {
                Flash();
                var targets = CombatState.HittableEnemies;
                await PowerCmd.Apply<BurnPower>(context, targets, Amount, this.Owner, null);
            }
            
        }
    }

}