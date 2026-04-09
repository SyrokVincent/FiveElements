using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

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
        
        // Check if the played card is a fire element card
        if (cardPlay.Card is FiveElementsCard elementCard && elementCard.IsFire())
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