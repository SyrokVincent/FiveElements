using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Uncommon;

public sealed class FireWeaving() : FireCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(CombatState);

    //Exhaust a non-fire card or itself if you can't. Deal 6 Heat damage, Fire:(return in hand if you exhausted a card)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(6,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Heat),
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ]);
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        // select of a non-fire card
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => !(c is FiveElementsCard f && f.IsFire()) && c != this,
            this
        );
        
        
        var selectedCard = selection?.FirstOrDefault();
        
        if (selectedCard != null)
        {
            // another card is exhausted
            await CardCmd.Exhaust(choiceContext, selectedCard);
            await DealHeatDamage(choiceContext, play.Target, DynamicVars.Damage);
            
        }
        else
        {
            //the card exhaust itself (returning to hand is done by getresultpile
            await DealHeatDamage(choiceContext, play.Target, DynamicVars.Damage);
            await CardCmd.Exhaust(choiceContext, this);
        }
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
    
    //this shit is called before onplay
    //change the pile to hand if fire is active (and also if the card don't exhaust itself)
    protected override PileType GetResultPileType()
    {
        PileType resultPileType = base.GetResultPileType();
        return (resultPileType is PileType.Discard or PileType.Exhaust &&
                CardElementTag.Fire.IsActive(CombatState)) ? PileType.Hand : resultPileType;
    }
    
    
      /*
    // Fire: return card to hand if it exhausted a card
    protected override PileType GetResultPileType()
    {
        GD.Print("getresultpiletype1, _logicHasExhausted: ",_logicHasExhausted);
        
        // 1. Condition de retour en main (Fire actif + une carte a été mangée)
        if (_logicHasExhausted && CombatState != null && CardElementTag.Fire.IsActive(CombatState))
        {
            GD.Print("getresultpiletype2, _logicHasExhausted: ",_logicHasExhausted);
            return PileType.Hand;
        }

        // 2. Si on n'a rien épuisé (hasExhausted == 0), la carte DOIT s'épuiser elle-même
        if (!_logicHasExhausted)
        {
            GD.Print("getresultpiletype3, _logicHasExhausted: ",_logicHasExhausted);
            return PileType.Exhaust;
        }

        GD.Print("getresultpiletype4, _logicHasExhausted: ",_logicHasExhausted);
        // 3. Cas par défaut (on a épuisé une carte mais pas de Fire actif)
        return PileType.Discard;
    }
    
  
    // Fire: return card to hand if it exhausted a card
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        PileType pileType,
        CardPilePosition position)
    {
        GD.Print("paaaaaaaaaaasse par la");
        if (card != this) {
            return base.ModifyCardPlayResultPileTypeAndPosition(card, isAutoPlay, resources, pileType, position);
        }
        
        bool hasExhausted = (DynamicVars["HasExhaustedAnotherCard"].BaseValue != 0);
        
        // Si l'élément FEU est actif ET qu'on a bien épuisé une carte durant le OnPlay
        if (hasExhausted && CombatState != null && CardElementTag.Fire.IsActive(CombatState))
        {
            AddKeyword(CardKeyword.Exhaust);
            return (PileType.Hand, CardPilePosition.Top);
        }
        return base.ModifyCardPlayResultPileTypeAndPosition(card, isAutoPlay, resources, pileType, position);
    }
    */
    
    /*
    public override async Task AfterCardPlayedLate(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != card.Owner || cardPlay.Resources.EnergyValue < card.DynamicVars.Energy.IntValue)
            return;
        CardPile pile = card.Pile;
        if ((pile != null ? (pile.Type != PileType.Discard ? 1 : 0) : 1) != 0)
            return;
        CardPileAddResult cardPileAddResult = await CardPileCmd.Add((CardModel) card, PileType.Hand);
    }
   */
    
}
