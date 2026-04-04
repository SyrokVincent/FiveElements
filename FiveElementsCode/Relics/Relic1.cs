using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public class Relic1() : FiveElementsRelic
{
    public static CardElementTag Echo = CardElementTag.Neutral;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        return player != Owner || player.Creature.CombatState.RoundNumber > 1 ? count : (count + DynamicVars.Cards.BaseValue);
    }

    //todo fiend  abetter way to track echo, couldnot manage to make it a CombatState "attribute" everywhere
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        //do not work to change card description color
        //check if it's our card being played
        if (this.Owner != cardPlay.Card.Owner)
        {
            //return Task.CompletedTask;
        }
        else
        {
            // On vérifie si la carte possède un composant d'élément
            if (cardPlay.Card is FiveElementsCard elementCard) {
                if (Echo != elementCard.ElementTags.Single()) {
                    Echo = elementCard.ElementTags.Single();
                    
                    //debug
                    // Affiche l'état global avant de notifier les cartes
                    var status = cardPlay.Card.CombatState.GetElementalStatus();
                    GD.Print($"DEBUG: after FEcard Echo={Relic1.Echo}, WaterEssence={status.GetEssence(CardElementTag.Water)}, " +
                             $"wood={status.GetEssence(CardElementTag.Wood)}, " +
                             $"fire={status.GetEssence(CardElementTag.Fire)}, " +
                             $"earth={status.GetEssence(CardElementTag.Earth)}, " +
                             $"metal={status.GetEssence(CardElementTag.Metal)}, " +
                             $"neutral={status.GetEssence(CardElementTag.Neutral)}");
                    
                    
                    
                    foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
                    {
                        _ = FiveElementsCardExtensions.CheckAndNotify(Owner.Creature.CombatState, elem);
                    }
                }
            } else {
                Echo = CardElementTag.Neutral;
                foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
                {
                    _ = FiveElementsCardExtensions.CheckAndNotify(Owner.Creature.CombatState, elem);
                }
            }

            GD.Print("Echooooo: " + Echo);
        }
        //base.AfterCardPlayed(context, cardPlay);
        //return Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        Echo = CardElementTag.Neutral;
        //debug
        // Affiche l'état global avant de notifier les cartes
        var status = Owner.Creature.CombatState.GetElementalStatus();
        GD.Print($"DEBUG: turnend Echo={Relic1.Echo}, WaterEssence={status.GetEssence(CardElementTag.Water)}, " +
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
        Echo = CardElementTag.Neutral;
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