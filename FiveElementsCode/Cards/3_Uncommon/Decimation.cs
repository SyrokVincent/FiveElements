using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class Decimation() : NeutralCard(5,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    //Retain, Reduce cost by 1 for each different element played this turn, deal 20
    //added retain (too hard to play without that?
    //added shift on upgrade since it's a card that want lot of element it makes sense to have it
    // rework shift by default and reduce cost by 1 when it changes element
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(20, ValueProp.Move)
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Shift,
        CardKeyword.Retain,
    ]);

    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Shift),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        if (CombatState == null) return;
        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }

/*
 //old method of cost reduction
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        //Only trigger if the owner of this card play a card
        if (Owner != cardPlay.Card.Owner) 
            return;
        
        
        // Calculer le nombre d'éléments distincts joués ce tour via le CACHE
        int distinctElements = CalculateDistinctElementsThisTurn();

        // On ajuste le coût : 5 de base moins les éléments trouvés (minimum 0)
        int newCost = Math.Max(0, 5 - distinctElements);
        
        this.EnergyCost.SetThisTurn(newCost);
    }
*/

    private HashSet<CardElementTag>? _tagsBeforePlay;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // On ne capture l'état que si la carte est actuellement en MAIN
        if (this.Pile?.Type == PileType.Hand && Owner == cardPlay.Card.Owner && cardPlay.Card != this)
        {
            _tagsBeforePlay = new HashSet<CardElementTag>(this.ElementTags);
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // Sécurité : on vérifie que le cache existe (donc qu'elle était en main au début)
        // ET on vérifie qu'elle est TOUJOURS en main (au cas où elle aurait été défaussée/jouée entre temps)
        if (_tagsBeforePlay == null || this.Pile?.Type != PileType.Hand) 
        {
            _tagsBeforePlay = null; // Reset par sécurité
            return Task.CompletedTask;
        }

        if (Owner != cardPlay.Card.Owner || cardPlay.Card == this) 
            return Task.CompletedTask;

        base.AfterCardPlayedLate(context, cardPlay);

        // Comparaison
        if (!_tagsBeforePlay.SetEquals(this.ElementTags))
        {
            GD.Print("[Decimation] Changement détecté en main : Réduction du coût.");
            this.EnergyCost.AddThisTurnOrUntilPlayed(-1);
        }

        _tagsBeforePlay = null;
        return Task.CompletedTask;
    }

    //old method used to reduce the cost
    private int CalculateDistinctElementsThisTurn()
    {
        var entries = CombatManager.Instance.History.CardPlaysStarted
            .Where(e => e.HappenedThisTurn(CombatState) && e.Actor.Player == Owner)
            .ToList();

        if (entries.Count == 0) return 0;

        var foundTags = new HashSet<CardElementTag>();
        var targetTags = new[] { 
            CardElementTag.Water, CardElementTag.Wood, CardElementTag.Fire, 
            CardElementTag.Earth, CardElementTag.Metal 
        };

        foreach (var entry in entries)
        {
            // On récupère les tags figés du cache
            if (NeutralCard.PlayedElementsCache.TryGetValue(entry.CardPlay, out var frozenTags))
            {
                foreach (var t in targetTags)
                {
                    if (frozenTags.TagsCountAsElement(t, Owner.Creature)) 
                        foundTags.Add(t);
                }
            }
            else
            {
                // Fallback pour les cartes Strike/Defend (non dynamiques)
                foreach (var t in targetTags)
                {
                    if (entry.CardPlay.Card.CountAsElement(t, Owner.Creature)) 
                        foundTags.Add(t);
                }
            }

            if (foundTags.Count >= 5) break;
        }

        return foundTags.Count;
    }
}