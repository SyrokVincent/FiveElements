using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class Annihilation() : NeutralCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)//, IOnElementStateChanged
{


    protected override bool ShouldGlowGoldInternal => CombatState != null && FiveElementsCardExtensions.IsAnyElementActive(CombatState);

    // Wood:(Deal 15), Fire:(Deal 15), Earth:(Deal 15), Metal:(Deal 15), Water:(Deal 15)
    //added shift on upgrade, buffed base dmg to 15
    // rework // Choose a card in your hand to gain it's essence. Deal 12 damage for each different essence you have!
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(12,ValueProp.Move),
        //new BoolVar("isWaterOn"),
        //new BoolVar("isWoodOn"),
        //new BoolVar("isFireOn"),
        //new BoolVar("isEarthOn"),
        //new BoolVar("isMetalOn"),
    ]);

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
            c => c is FiveElementsCard f && f.ElementTags.Any(t => t != CardElementTag.Neutral) && c != this,
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
                if (CombatState != null) CombatState.GetElementalStatus().AddEssence(tag, 1,choiceContext);
                GD.Print($"Essence ajoutée ! Élément : {tag}");
            }
        }
        
        if (CombatState != null)
        {
            var essences = CombatState.GetElementalStatus();
            var count = 0;
            if (essences.GetEssence(CardElementTag.Water)>0)
            {
                count++;
            }
            if (essences.GetEssence(CardElementTag.Wood)>0)
            {
                count++;
            }   
            if (essences.GetEssence(CardElementTag.Fire)>0)
            {
                count++;
            }
            if (essences.GetEssence(CardElementTag.Earth)>0)
            {
                count++;
            }
            if (essences.GetEssence(CardElementTag.Metal)>0)
            {
                count++;
            }
            if (count!=0)
            {
                await CommonActions.CardAttack(this, play.Target,count).Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        AddKeyword(FiveElementsKeywords.Shift);
    }
    
    /*
    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        string? varName = element switch
        {
            CardElementTag.Water => "isWaterOn",
            CardElementTag.Wood => "isWoodOn",
            CardElementTag.Fire => "isFireOn",
            CardElementTag.Earth => "isEarthOn",
            CardElementTag.Metal => "isMetalOn",
            _ => null
        };

        if (varName != null)
        {
            DynamicVars[varName].BaseValue = isActive ? 1 : 0;
        }
        await Task.CompletedTask;
    }
    */
}