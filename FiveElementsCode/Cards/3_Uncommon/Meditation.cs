using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class Meditation() : NeutralCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{


    //Select 1 element card, gain it's essence and draw 3+1
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(3),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Essence),
    ]);
    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        
        if (CombatState == null) return;
        
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
    
        if (selectedModel is FiveElementsCard card)
        {
            // will cause chaos when some card will have multiple element?
            // On boucle sur tous les tags de la carte choisie
            // On ignore le Neutre, et on ajoute 1 essence pour chaque autre tag trouvé
            foreach (var tag in card.ElementTags.Where(tag => tag != CardElementTag.Neutral))
            {
                if (CombatState != null) CombatState.GetElementalStatus().AddEssence(tag, 1);
                GD.Print($"Essence ajoutée ! Élément : {tag}");
            }
        }
        
        // 4 draw cards
        await CommonActions.Draw(this, choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
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