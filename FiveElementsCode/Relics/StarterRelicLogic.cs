using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._6_Ancient;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace FiveElements.FiveElementsCode.Relics;

public abstract class StarterRelicLogic : FiveElementsRelic
{
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        // On vérifie si c'est le premier tour
        if (player == Owner && player.Creature.CombatState is { RoundNumber: 1 })
        {
            //reset de echo et des essences au cas ou on save and exit???
            Character.FiveElements.ResetEcho();
            combatState.GetElementalStatus().ResetAllEssences();
            foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
            {
                _ = FiveElementsCardExtensions.CheckAndNotify(combatState, elem);
            }
            
            await Task.CompletedTask;
        }
    }
    
    //should make all card generated from potion have their description working
    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card is FiveElementsCard feCard)
        {
            await FiveElementsCardExtensions.SyncElementalState(feCard, this.Owner.Creature.CombatState);
        }
        await base.AfterCardGeneratedForCombat(card, creator);
    }

    // On stocke les éléments que la carte "avait" au moment du clic
    private HashSet<CardElementTag> _cardElementsBeforePlay = new();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        
        // 2. Snapshot des éléments de la carte AVANT qu'elle ne change
        _cardElementsBeforePlay.Clear();
        var card = cardPlay.Card;

        // On vérifie tous les éléments possibles via CountAsElement
        // car cela inclut Attune/Shift calculé au moment T
        if (card.CountAsElement(CardElementTag.Water, Owner.Creature)) _cardElementsBeforePlay.Add(CardElementTag.Water);
        if (card.CountAsElement(CardElementTag.Wood, Owner.Creature))  _cardElementsBeforePlay.Add(CardElementTag.Wood);
        if (card.CountAsElement(CardElementTag.Fire, Owner.Creature))  _cardElementsBeforePlay.Add(CardElementTag.Fire);
        if (card.CountAsElement(CardElementTag.Earth, Owner.Creature)) _cardElementsBeforePlay.Add(CardElementTag.Earth);
        if (card.CountAsElement(CardElementTag.Metal, Owner.Creature)) _cardElementsBeforePlay.Add(CardElementTag.Metal);

        return Task.CompletedTask;
    }
    
    //todo find a better way to track echo, couldnot manage to make it a CombatState "attribute" everywhere
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        //do not work to change card description color
        //check if it's our card being played
        if (this.Owner != cardPlay.Card.Owner) return;
      
        var cardWas = _cardElementsBeforePlay;
        
        // On vérifie si la carte possède un composant d'élément
        if (cardPlay.Card is FiveElementsCard elementCard) {
            if (!Character.FiveElements.Echo.SetEquals(cardWas)) {
                if (elementCard.IsNeutral() && Owner.Creature.HasPower<SpiritsFormPower>() && cardPlay.Card is not SpiritsForm)
                {
                    Character.FiveElements.SetEchoToAllElements();
                }
                else
                {
                    Character.FiveElements.Echo = cardWas.ToHashSet();
                }
                
                
                
                //objectif refresh l'icone de l'energy lorsque echo change pour montrer l'echo visuelement
                // mais apparament c'est deja appeler ailleurs!!
                /*
                List<CardModel> cardsInHand = PileType.Hand.GetPile(elementCard.Owner).Cards.ToList<CardModel>();
                GD.Print("card in hand:? : " + cardsInHand.Count);
                foreach (var card in cardsInHand) {
                    // On demande au moteur de notifier que la vue doit changer
                    card.InvokeEnergyCostChanged();
                    GD.Print(card.Title);
                }
                GD.Print("Pool Echo mis à jour vers : " + Character.FiveElements.Echo);
                */
                
                //debug
                // Affiche l'état global avant de notifier les cartes
                string echoContent = string.Join(", ", Character.FiveElements.Echo);
                var status = cardPlay.Card.CombatState.GetElementalStatus();
                GD.Print($"DEBUG: after FEcard Echo=[{echoContent}], " +
                         $"WaterEssence={status.GetEssence(CardElementTag.Water)}, " +
                         $"wood={status.GetEssence(CardElementTag.Wood)}, " +
                         $"fire={status.GetEssence(CardElementTag.Fire)}, " +
                         $"earth={status.GetEssence(CardElementTag.Earth)}, " +
                         $"metal={status.GetEssence(CardElementTag.Metal)}, " +
                         $"neutral={status.GetEssence(CardElementTag.Neutral)}");
                
                
                
                foreach (CardElementTag elem in Enum.GetValues<CardElementTag>())
                {
                    _ = FiveElementsCardExtensions.CheckAndNotify(Owner.Creature.CombatState, elem);
                }
            }
        } else {  //on est entrain de jouer une carte de base
            if(Owner.Creature.HasPower<SpiritsFormPower>())
            {
                Character.FiveElements.SetEchoToAllElements();
            }
            else
            {
                Character.FiveElements.ResetEcho();
            }
            
            foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
            {
                _ = FiveElementsCardExtensions.CheckAndNotify(Owner.Creature.CombatState, elem);
            }
        }

            //GD.Print("Echooooo: " + Character.FiveElements.Echo);
        
        //base.AfterCardPlayed(context, cardPlay);
        //return Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        Character.FiveElements.ResetEcho();
        //debug
        // Affiche l'état global avant de notifier les cartes
        string echoContent = string.Join(", ", Character.FiveElements.Echo);
        var status = Owner.Creature.CombatState.GetElementalStatus();
        GD.Print($"DEBUG: turnend Echo=[{echoContent}], " +
                 $"WaterEssence={status.GetEssence(CardElementTag.Water)}, " +
                 $"wood={status.GetEssence(CardElementTag.Wood)}, " +
                 $"fire={status.GetEssence(CardElementTag.Fire)}, " +
                 $"earth={status.GetEssence(CardElementTag.Earth)}, " +
                 $"metal={status.GetEssence(CardElementTag.Metal)}, " +
                 $"neutral={status.GetEssence(CardElementTag.Neutral)}");
        
        foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
        {
            _ = FiveElementsCardExtensions.CheckAndNotify(Owner.Creature.CombatState, elem);
        }
        //base.BeforeTurnEnd(choiceContext, side);
        //return Task.CompletedTask;
    }
   

    //todo need to do that at a better place, does'nt work when you give up and restart for example
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        Character.FiveElements.ResetEcho();
        room.CombatState.GetElementalStatus().ResetAllEssences();
        foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
        {
            _ = FiveElementsCardExtensions.CheckAndNotify(room.CombatState, elem);
        }
        await Task.CompletedTask;
        // async or that idk what I need to do
        //base.AfterCombatEnd(room);
        //return Task.CompletedTask;
    }
}