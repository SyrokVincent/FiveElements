using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Character;
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
                    //RefreshDeck(Owner.Deck.Cards);
                    Echo = elementCard.ElementTags.Single();
                }
            } else {
                Echo = CardElementTag.Neutral;
                //RefreshDeck(Owner.Deck.Cards);
            }

            GD.Print("Echooooo: " + Echo);
        }
        //base.AfterCardPlayed(context, cardPlay);
        //return Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        Echo = CardElementTag.Neutral;
        //RefreshDeck(Owner.Deck.Cards);
        //base.BeforeTurnEnd(choiceContext, side);
        //return Task.CompletedTask;
    }

    private void RefreshDeck(IReadOnlyList<CardModel> deck)
    {
        foreach (var c in deck)
        {
            if (c is not FiveElementsCard ce) continue;
            if (ce.DynamicVars.TryGetValue("water_s", out var baseVar1)){
                if (baseVar1 is StringVar myVar1){
                    myVar1.StringValue = CardElementTag.Water.IsActive() ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("water_e", out var baseVar2)){
                if (baseVar2 is StringVar myVar2){
                    myVar2.StringValue = CardElementTag.Water.IsActive() ? "" : "[/color]";
                }
            }if (ce.DynamicVars.TryGetValue("wood_s", out var baseVar3)){
                if (baseVar3 is StringVar myVar3){
                    myVar3.StringValue = CardElementTag.Wood.IsActive() ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("wood_e", out var baseVar4)){
                if (baseVar4 is StringVar myVar4){
                    myVar4.StringValue = CardElementTag.Wood.IsActive() ? "" : "[/color]";
                }
            }
            if (ce.DynamicVars.TryGetValue("fire_s", out var baseVar5)){
                if (baseVar5 is StringVar myVar5){
                    myVar5.StringValue = CardElementTag.Fire.IsActive() ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("fire_e", out var baseVar6)){
                if (baseVar6 is StringVar myVar6){
                    myVar6.StringValue = CardElementTag.Fire.IsActive() ? "" : "[/color]";
                }
            }
            if (ce.DynamicVars.TryGetValue("earth_s", out var baseVar7)){
                if (baseVar7 is StringVar myVar7){
                    myVar7.StringValue = CardElementTag.Earth.IsActive() ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("earth_e", out var baseVar8)){
                if (baseVar8 is StringVar myVar8){
                    myVar8.StringValue = CardElementTag.Earth.IsActive() ? "" : "[/color]";
                }
            }
            if (ce.DynamicVars.TryGetValue("metal_s", out var baseVar9)){
                if (baseVar9 is StringVar myVar9){
                    myVar9.StringValue = CardElementTag.Metal.IsActive() ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("metal_e", out var baseVar10)){
                if (baseVar10 is StringVar myVar10){
                    myVar10.StringValue = CardElementTag.Metal.IsActive() ? "" : "[/color]";
                }
            }
            
        }
    }

    //todo need to do that at a better place, does'nt work when you give up and restart for example
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        Echo = CardElementTag.Neutral;
        FiveElementsCard.WaterEnergy = 0;
        FiveElementsCard.WoodEnergy = 0;
        FiveElementsCard.FireEnergy = 0;
        FiveElementsCard.EarthEnergy = 0;
        FiveElementsCard.MetalEnergy = 0;
        // async or that idk what I need to do
        //base.AfterCombatEnd(room);
        //return Task.CompletedTask;
    }
}