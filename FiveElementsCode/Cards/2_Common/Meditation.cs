using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class Meditation() : NeutralCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{


    //Discard 1 card with Element, put 2 random card with a different Element from the discard pile into your hand
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(2),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        //HoverTipFactory.FromPower<WavePower>(),
    ]);

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
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}