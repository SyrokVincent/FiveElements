using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class AllOrOnePower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // recuperation des cartes jouée le tour precedent
        var lastTurnPlayedCards = CombatManager.Instance.History.CardPlaysStarted
            .Where(e => e.RoundNumber == (CombatState.RoundNumber - 1) && e.CardPlay.Card.Owner == player)
            .Select(e => e.CardPlay.Card)
            .OfType<FiveElementsCard>()
            .ToList();
        
        if (lastTurnPlayedCards.Count == 0) return;

        // Extraire tous les éléments uniques joués (en ignorant le Neutre)
        var uniqueElements = lastTurnPlayedCards
            .SelectMany(c => c.ElementTags)
            .Where(t => t != CardElementTag.Neutral)
            .Distinct()
            .ToList();

        // Vérification : TOUS DU MÊME ÉLÉMENT
        bool allSameElement = uniqueElements.Count == 1;

        // Vérification : TOUS LES ÉLÉMENTS 
        bool allElementsPlayed = uniqueElements.Count >= 5; 
        
        if (allElementsPlayed || allSameElement)
        {
            Flash();
            if (Owner.Player != null)
            {
                await PlayerCmd.GainEnergy( Amount, Owner.Player);
                await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
            }
        }
        
    }
}