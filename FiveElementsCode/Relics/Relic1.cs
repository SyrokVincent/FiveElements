using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public class Relic1() : FiveElementsRelic
{

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<Creation>(),
    ]); 
    
    //modifie plus le hand draw du debut  
    /*
    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        return player != Owner || player.Creature.CombatState.RoundNumber > 1 ? count : (count + DynamicVars.Cards.BaseValue);
        
    }*/

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        // On vérifie si c'est le premier tour
        if (player == Owner && player.Creature.CombatState is { RoundNumber: 1 })
        {
            await FiveElementsCardExtensions.CreateInHand<Creation>(Owner, 1,false, combatState);
        }
    }

    
    
    //should make all card generated from potion have their description working
    public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
    {
        if (card is FiveElementsCard feCard)
        {
            await FiveElementsCardExtensions.SyncElementalState(feCard, this.Owner.Creature.CombatState);
        }
        await base.AfterCardGeneratedForCombat(card, addedByPlayer);
    }
    
    // On stocke les éléments que la carte "avait" au moment du clic
    private HashSet<CardElementTag> _cardElementsBeforePlay = new();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        
        // 2. Snapshot des éléments de la carte AVANT qu'elle ne change
        _cardElementsBeforePlay.Clear();
        var card = cardPlay.Card;

        // On vérifie tous les éléments possibles via CountsAsElement
        // car cela inclut Attune/Shift calculé au moment T
        if (card.CountsAsElement(CardElementTag.Water, Owner.Creature)) _cardElementsBeforePlay.Add(CardElementTag.Water);
        if (card.CountsAsElement(CardElementTag.Wood, Owner.Creature))  _cardElementsBeforePlay.Add(CardElementTag.Wood);
        if (card.CountsAsElement(CardElementTag.Fire, Owner.Creature))  _cardElementsBeforePlay.Add(CardElementTag.Fire);
        if (card.CountsAsElement(CardElementTag.Earth, Owner.Creature)) _cardElementsBeforePlay.Add(CardElementTag.Earth);
        if (card.CountsAsElement(CardElementTag.Metal, Owner.Creature)) _cardElementsBeforePlay.Add(CardElementTag.Metal);

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
                if (elementCard.IsNeutral() && Owner.Creature.HasPower<SpiritsFormPower>())
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
            _ = FiveElementsCardExtensions.CheckAndNotify(Owner.Creature.CombatState, elem);
        }
        await Task.CompletedTask;
        // async or that idk what I need to do
        //base.AfterCombatEnd(room);
        //return Task.CompletedTask;
    }
}