using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Character;
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

    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        return player != Owner || player.Creature.CombatState.RoundNumber > 1
            ? count
            : count + DynamicVars.Cards.BaseValue;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        //do not work to change card description color
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

    
        base.AfterCardPlayed(context, cardPlay);
        return Task.CompletedTask;
    }

    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        Echo = CardElementTag.Neutral;
        //RefreshDeck(Owner.Deck.Cards);
        base.BeforeTurnEnd(choiceContext, side);
        return Task.CompletedTask;
    }

    private void RefreshDeck(IReadOnlyList<CardModel> deck)
    {
        foreach (var c in deck)
        {
            if (c is not FiveElementsCard ce) continue;
            if (ce.DynamicVars.TryGetValue("water_s", out var baseVar1)){
                if (baseVar1 is StringVar myVar1){
                    myVar1.StringValue = ce.IsElementActive(CardElementTag.Water) ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("water_e", out var baseVar2)){
                if (baseVar2 is StringVar myVar2){
                    myVar2.StringValue = ce.IsElementActive(CardElementTag.Water) ? "" : "[/color]";
                }
            }if (ce.DynamicVars.TryGetValue("wood_s", out var baseVar3)){
                if (baseVar3 is StringVar myVar3){
                    myVar3.StringValue = ce.IsElementActive(CardElementTag.Wood) ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("wood_e", out var baseVar4)){
                if (baseVar4 is StringVar myVar4){
                    myVar4.StringValue = ce.IsElementActive(CardElementTag.Wood) ? "" : "[/color]";
                }
            }
            if (ce.DynamicVars.TryGetValue("fire_s", out var baseVar5)){
                if (baseVar5 is StringVar myVar5){
                    myVar5.StringValue = ce.IsElementActive(CardElementTag.Fire) ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("fire_e", out var baseVar6)){
                if (baseVar6 is StringVar myVar6){
                    myVar6.StringValue = ce.IsElementActive(CardElementTag.Fire) ? "" : "[/color]";
                }
            }
            if (ce.DynamicVars.TryGetValue("earth_s", out var baseVar7)){
                if (baseVar7 is StringVar myVar7){
                    myVar7.StringValue = ce.IsElementActive(CardElementTag.Earth) ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("earth_e", out var baseVar8)){
                if (baseVar8 is StringVar myVar8){
                    myVar8.StringValue = ce.IsElementActive(CardElementTag.Earth) ? "" : "[/color]";
                }
            }
            if (ce.DynamicVars.TryGetValue("metal_s", out var baseVar9)){
                if (baseVar9 is StringVar myVar9){
                    myVar9.StringValue = ce.IsElementActive(CardElementTag.Metal) ? "" : "[color=#666666]";
                }
            }
            if (ce.DynamicVars.TryGetValue("metal_e", out var baseVar10)){
                if (baseVar10 is StringVar myVar10){
                    myVar10.StringValue = ce.IsElementActive(CardElementTag.Metal) ? "" : "[/color]";
                }
            }
            
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        //not really sur it's needed
        FiveElementsCard.WaterEnergy = 0;
        FiveElementsCard.WoodEnergy = 0;
        FiveElementsCard.FireEnergy = 0;
        FiveElementsCard.EarthEnergy = 0;
        FiveElementsCard.MetalEnergy = 0;
        base.AfterCombatEnd(room);
        return Task.CompletedTask;
    }
}