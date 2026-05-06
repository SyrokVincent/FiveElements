using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class Recycle() : NeutralCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{

  
    //Choose a card in your hand to Transform into a card of the element it generate, (choose between 3? maybe broken it's 3 from 13)
    //
    //added:  it cost 1 less this turn
    // change on upgrade -retain + shift
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
    ]);
    
    
    private CardElementTag GetGeneratedElement(CardElementTag tag)
    {
        
            if (tag == CardElementTag.Water) return CardElementTag.Wood;
            if (tag == CardElementTag.Wood ) return CardElementTag.Fire;
            if (tag == CardElementTag.Fire) return CardElementTag.Earth;
            if (tag == CardElementTag.Earth) return CardElementTag.Metal;
            if (tag == CardElementTag.Metal) return CardElementTag.Water;
            return CardElementTag.Neutral;
    }
        
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        if (CombatState == null) return;

        // 1. Sélection de la carte à recycler dans la main
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1);
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => c is FiveElementsCard f && f.ElementTags.Any(t => t != CardElementTag.Neutral) && c != this,
            this
        );

        var targetCard = selection?.FirstOrDefault();
        if (targetCard == null || targetCard is not FiveElementsCard fec) return;

        // 2. Déterminer l'élément cible
        // On récupère le premier élément de la carte pour définir la destination
        var sourceElement = fec.ElementTags.FirstOrDefault(t => t != CardElementTag.Neutral);
        var targetElement = GetGeneratedElement(sourceElement);

        // 3. Préparer le pool de cartes filtré par cet élément
        var fullPool = ModelDb.CardPool<FiveElementsCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint);
        
        // On ne garde que les cartes de l'élément généré
        var filteredPool = fullPool
            .Where(c => c is FiveElementsCard f && f.CanonicalElementTags.Contains(targetElement))
            .ToList();

        // 4. Générer les x choix (Discover)
        var choices = CardFactory.GetDistinctForCombat(Owner, filteredPool, DynamicVars.Cards.IntValue, Owner.RunState.Rng.CombatCardGeneration).ToList();

        if (choices.Count == 0) return;

        // Si la carte sacrifiée était améliorée, on améliore les choix
        if (targetCard.IsUpgraded)
        {
            if (targetCard.IsUpgraded)
            {
                CardCmd.Upgrade(choices, CardPreviewStyle.HorizontalLayout);
            }
        }
        
        CardModel? selectedChoice;
        if (choices.Count == 1)
        {
            selectedChoice = choices.FirstOrDefault();

            if (selectedChoice != null)
            {
                await FiveElementsCardExtensions.TransformInHand(targetCard, selectedChoice, false,Owner.Creature);
                selectedChoice.EnergyCost.AddThisTurnOrUntilPlayed(-1);
            }
        }
        else
        {
            // Écran de choix
            selectedChoice = await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, Owner, false);
        
            if (selectedChoice != null)
            {
                await FiveElementsCardExtensions.TransformInHand(targetCard, selectedChoice, false,Owner.Creature);
                selectedChoice.EnergyCost.AddThisTurnOrUntilPlayed(-1);
            }
        }
        
    }
    
    protected override void OnUpgrade()
    {
        this.AddKeyword(FiveElementsKeywords.Shift);
    }
}