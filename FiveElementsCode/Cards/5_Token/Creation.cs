using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace FiveElements.FiveElementsCode.Cards._5_Token;

[Pool(typeof(TokenCardPool))]
public sealed class Creation() : NeutralCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    //Innate, Ethereal,Exhaust, Select one card in hand and gain 1 "Essence" of it
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        FiveElementsKeywords.Essence,
        CardKeyword.Innate, 
        CardKeyword.Ethereal, 
        CardKeyword.Exhaust, 
    ];
    //protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory. ];

  
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
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
            c => c is FiveElementsCard f && !f.IsNeutral(), 
            this
        );

        // 3. Vérifier si une carte a bien été choisie
        var selectedModel = selection?.FirstOrDefault();
    
        if (selectedModel is FiveElementsCard card)
        {
            //todo will cause chaos when some card will have multiple element
            // On boucle sur tous les tags de la carte choisie
            // On ignore le Neutre, et on ajoute 1 essence pour chaque autre tag trouvé
            foreach (var tag in card.ElementTags.Where(tag => tag != CardElementTag.Neutral))
            {
                if (CombatState != null) CombatState.GetElementalStatus().AddEssence(tag, 1);
                GD.Print($"Essence ajoutée ! Élément : {tag}");
            }
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
        AddKeyword(CardKeyword.Retain);
    }
}