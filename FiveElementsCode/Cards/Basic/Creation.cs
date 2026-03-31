using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Basic;

public class Creation() : FiveElementsCard(0,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Innate, CardKeyword.Ethereal];
   
    protected override HashSet<CardElementTag> CanonicalElementTags => [CardElementTag.Neutral];  
    //protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory. ];

  
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        GD.Print($"PortraitPaaaaaaaaaath: {PortraitPath}");
        
        
        
        // 1. Préparer les préférences
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);

        // 2. Lancer la commande de sélection
        // Note : on enlève le cast direct au début pour éviter les erreurs
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => c is FiveElementsCard f && !f.ElementTags.Contains(CardElementTag.Neutral), 
            this
        );

        // 3. Vérifier si une carte a bien été choisie
        var selectedModel = selection?.FirstOrDefault();
    
        if (selectedModel is FiveElementsCard card)
        {
            // 4. Appliquer la logique
            if (card.ElementTags.Contains(CardElementTag.Water))
            {
                WaterEnergy += 1;
            }
            else if (card.ElementTags.Contains(CardElementTag.Wood))
            {
                WoodEnergy += 1;
            }   
            else if (card.ElementTags.Contains(CardElementTag.Fire))
            {
                FireEnergy += 1;
            }
            else if (card.ElementTags.Contains(CardElementTag.Earth))
            {
                EarthEnergy += 1;
            }
            else if (card.ElementTags.Contains(CardElementTag.Metal))
            {
                MetalEnergy += 1;
            }
        
            GD.Print($"Énergie ajoutée ! Élément : {card.ElementTags.Single()}");
        }
        else 
        {
            GD.Print("Aucune carte valide sélectionnée ou sélection annulée.");
        }
        
        /*
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        FiveElementsCard card = (FiveElementsCard)(await CardSelectCmd.FromHand(choiceContext, Owner, prefs, 
            (Func<CardModel, bool>) (c => c is FiveElementsCard fCard && !fCard.ElementTags.Contains(CardElementTag.Neutral)),this)).FirstOrDefault<CardModel>();
       
        if (card == null)
            return;
        if (card.ElementTags.Contains(CardElementTag.Water))
        {
            WaterEnergy += 1;
        }
        else if (card.ElementTags.Contains(CardElementTag.Wood))
        {
            WoodEnergy += 1;
        }   
        else if (card.ElementTags.Contains(CardElementTag.Fire))
        {
            FireEnergy += 1;
        }
        else if (card.ElementTags.Contains(CardElementTag.Earth))
        {
            EarthEnergy += 1;
        }
        else if (card.ElementTags.Contains(CardElementTag.Metal))
        {
            MetalEnergy += 1;
        }
        */
    }//var cardModel = (FiveElementsCard) await CommonActions.SelectSingleCard(this, SelectionScreenPrompt, choiceContext, PileType.Hand);


    protected override void OnUpgrade()
    {

    }
}