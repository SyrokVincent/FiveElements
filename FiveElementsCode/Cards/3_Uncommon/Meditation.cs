using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class Meditation() : NeutralCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{


    //(old)Select 1 element card, gain it's essence and draw 3+1
    //
    //(new) Draw 1, Select 1 element card, trigger the corresponding effect on the card Activation, 
    //
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        //need that if i ever want it to draw 2 lol
        //new CardsVar("Draw",1),
        
        //water
        ActivationVars.Energy,
        ActivationVars.Wave,
        //wood
        ActivationVars.Cards,
        ActivationVars.TempStrength,
        //fire
        ActivationVars.Burn,
        //earth
        ActivationVars.Block,
        //metal
        ActivationVars.Vigor,
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<Activation>(),
    ]);
    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        
        if (CombatState == null) return;
        
       
        await CardPileCmd.Draw(choiceContext, 1, Owner);
        
        // 1. Préparer les préférences
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);

        // 2. Lancer la commande de sélection
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => c is FiveElementsCard f && f.ElementTags.Any(t => t != CardElementTag.Neutral), 
            this
        );

        // 3. Vérifier si une carte a bien été choisie
        var selectedModel = selection?.FirstOrDefault();
    
        if (selectedModel == null ) return;

        // 1. Déclenchement des effets selon l'élément
        if (selectedModel.CountAsElement(CardElementTag.Water,Owner.Creature))
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            await PowerCmd.Apply<WavePower>(Owner.Creature, DynamicVars["WavePower"].BaseValue, Owner.Creature, null);
        }
        if (selectedModel.CountAsElement(CardElementTag.Wood, Owner.Creature))
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
            await PowerCmd.Apply<ActivationTempStrengthPower>(Owner.Creature, DynamicVars["ActivationTempStrengthPower"].BaseValue, Owner.Creature, null);
        }
        if (selectedModel.CountAsElement(CardElementTag.Fire, Owner.Creature))
        {
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<BurnPower>(targets, this.DynamicVars["BurnPower"].BaseValue, this.Owner.Creature, null);
        }
        if (selectedModel.CountAsElement(CardElementTag.Earth, Owner.Creature))
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, DynamicVars.Block.Props, null);

        }
        if (selectedModel.CountAsElement(CardElementTag.Metal, Owner.Creature))
        {
            await PowerCmd.Apply<VigorPower>(Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, null);
        }
        
    }

    protected override void OnUpgrade()
    {
        AddKeyword(FiveElementsKeywords.Attune);
    }
    
    
    // old behavior
    /*
  protected override async Task OnPlay(
      PlayerChoiceContext choiceContext,
      CardPlay play)
  {
      await base.OnPlay(choiceContext, play);

      if (CombatState == null) return;
      var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1);

      // select of an elem card
      var selection = await CardSelectCmd.FromHand(
          choiceContext,
          Owner,
          prefs,
          c => c is FiveElementsCard f && !f.IsNeutral() && c != this,
          this
      );


      var selectedCard = selection?.FirstOrDefault();
      var elemOfSelectedCard = CardElementTag.Neutral;
      if (selectedCard != null)
      {
          if (selectedCard is FiveElementsCard fec) elemOfSelectedCard = fec.ElementTags.LastOrDefault();
          await CardCmd.Discard(choiceContext, selectedCard);
      }

      // 1. Récupérer toutes les cartes d'element different dans la défausse
      var discardPile = PileType.Discard.GetPile(Owner).Cards;
      var validCardsInDiscard =
          discardPile.Where(c =>
              c is FiveElementsCard fec &&
              !fec.IsNeutral() &&
              !fec.IsElement(elemOfSelectedCard))
              .ToList();

      // 2. Mélanger la liste de manière "instable" (aléatoire) en utilisant le RNG du jeu
      var cardsToRetrieve = validCardsInDiscard
          .UnstableShuffle(Owner.RunState.Rng.CombatCardSelection)
          .Take(DynamicVars.Cards.IntValue);

      foreach (var card in cardsToRetrieve)
      {
          await CardPileCmd.Add(card, PileType.Hand);
      }
  }*/
}