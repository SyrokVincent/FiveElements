using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class MetalEdgeDiscountPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);

    
    // Cette méthode change visuellement le coût de la carte en combat
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        // On ne modifie que si c'est notre carte, que c'est une attaque, 
        // et qu'elle est soit dans la main, soit en train d'être jouée.
        if (card.Owner.Creature != Owner || card.Type != CardType.Attack)
            return false;

        if (card.Pile?.Type is not (PileType.Hand or PileType.Play))
            return false;

        modifiedCost = modifiedCost - Amount;
        return true;
    }

    // Cette méthode consomme le pouvoir juste avant que la carte ne soit jouée
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Attack)
            return;

        if (cardPlay.Card.Pile?.Type is not (PileType.Hand or PileType.Play))
            return;
        
        await PowerCmd.Remove(this);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;
        await PowerCmd.Remove(this);
    }

}