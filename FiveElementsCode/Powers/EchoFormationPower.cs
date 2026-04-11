using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class EchoFormationPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
    ];

    public override Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
    {
        // On vérifie que le propriétaire du pouvoir est bien le joueur
        // et que le jeu s'apprête effectivement à défausser la main (Flush)
        if (player.Creature.CombatState != null && (player != Owner.Player || !Hook.ShouldFlush(player.Creature.CombatState, player)))
        {
            return Task.CompletedTask;
        }

        if (Owner.Player == null) return Task.CompletedTask;
        
        var handCards = PileType.Hand.GetPile(Owner.Player).Cards;

        // On compte les cartes echo
        var echoCardsInHand = handCards.Where(c => 
                c is FiveElementsCard fec && 
                !fec.IsNeutral() &&
                fec.IsElement(Character.FiveElements.Echo) && 
                !fec.ShouldRetainThisTurn // no need to take card that already have retain
        );

        // On applique l'effet de Retain sur chaque carte echo
        foreach (CardModel card in echoCardsInHand)
        {
            card.GiveSingleTurnRetain();
        }

        return Task.CompletedTask;
    }

}