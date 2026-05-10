using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
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

    /*
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
    */

    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        this.Flash();
        await CardPileCmd.Draw(choiceContext, (int)Amount, player);
        
        // 1. Récupération des entrées du tour précédent
        // On utilise CardPlaysFinished pour être sûr que la carte a bien fini son effet
        var lastTurnEntries = CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.RoundNumber == (CombatState.RoundNumber - 1) && e.Actor.Player == player)
            .ToList();
        
        if (lastTurnEntries.Count == 0) return;

        var uniqueElements = new SortedSet<CardElementTag>();

        foreach (var entry in lastTurnEntries)
        {
            var elementStatus = Owner.GetElementalStatus();
            // PRIORITÉ : Le cache figé (contient l'état exact au moment du clic)
            if (elementStatus.PlayedElementsCache.TryGetValue(entry.CardPlay, out var frozenTags))
            {
                // On utilise notre extension TagsCountAsElement pour gérer SpiritsForm 
                // sur les tags qui étaient présents à ce moment-là.
                if (frozenTags.TagsCountAsElement(CardElementTag.Water, Owner)) uniqueElements.Add(CardElementTag.Water);
                if (frozenTags.TagsCountAsElement(CardElementTag.Wood, Owner))  uniqueElements.Add(CardElementTag.Wood);
                if (frozenTags.TagsCountAsElement(CardElementTag.Fire, Owner))  uniqueElements.Add(CardElementTag.Fire);
                if (frozenTags.TagsCountAsElement(CardElementTag.Earth, Owner)) uniqueElements.Add(CardElementTag.Earth);
                if (frozenTags.TagsCountAsElement(CardElementTag.Metal, Owner)) uniqueElements.Add(CardElementTag.Metal);
            }
            else 
            {
                // FALLBACK : Uniquement pour les cartes de base (Strike/Defend) 
                // qui n'ont PAS de mécanique d'élément dynamique.
                if (entry.CardPlay.Card.CountAsElement(CardElementTag.Water, Owner)) uniqueElements.Add(CardElementTag.Water);
                if (entry.CardPlay.Card.CountAsElement(CardElementTag.Wood, Owner))  uniqueElements.Add(CardElementTag.Wood);
                if (entry.CardPlay.Card.CountAsElement(CardElementTag.Fire, Owner))  uniqueElements.Add(CardElementTag.Fire);
                if (entry.CardPlay.Card.CountAsElement(CardElementTag.Earth, Owner)) uniqueElements.Add(CardElementTag.Earth);
                if (entry.CardPlay.Card.CountAsElement(CardElementTag.Metal, Owner)) uniqueElements.Add(CardElementTag.Metal);
            }
            
            // --- OPTIMISATION ---
            // Si on a déjà les 5 éléments, plus besoin de regarder les autres cartes !
            if (uniqueElements.Count >= 5) 
                break;
        }

        if (uniqueElements.Count == 0) return;

        // Logique de calcul
        bool allSameElement = uniqueElements.Count == 1;
        bool allElementsPlayed = uniqueElements.Count >= 5; 
        
        if (allElementsPlayed || allSameElement)
        {
            this.Flash();
            await PlayerCmd.GainEnergy((int)Amount, player);
            //await CardPileCmd.Draw(choiceContext, (int)Amount, player);
        }
    }
}