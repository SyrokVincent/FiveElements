using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class Circulation() : NeutralCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && HasValidTarget();

    protected override bool ShouldGlowRedInternal => CombatState != null && !HasValidTarget();
    
    private bool HasValidTarget()
    {
        // On récupère la main via la méthode d'extension standard
        var hand = PileType.Hand.GetPile(Owner).Cards;

        // On cherche s'il existe une carte (autre que celle-ci) 
        // qui correspond à tes critères de "Circulation"
        return hand.Any(c => c != this && IsValidCirculationTarget(c));
    }

    private bool IsValidCirculationTarget(CardModel card)
    {
        // Ici, définis ta logique de filtrage. 
        // Par exemple, si tes cartes Echo/Generate héritent de FiveElementsCard :
        var currentEcho = Character.FiveElements.Echo;
        if (card is FiveElementsCard fec)
        {
            if (currentEcho == CardElementTag.Water && fec.IsWood()) return true;
            if (currentEcho == CardElementTag.Wood && fec.IsFire()) return true;
            if (currentEcho == CardElementTag.Fire && fec.IsEarth()) return true;
            if (currentEcho == CardElementTag.Earth && fec.IsMetal()) return true;
            if (currentEcho == CardElementTag.Metal && fec.IsWater()) return true;
        }
        return false;
    }


    //Select a card in hand that Echo generate, gain 1 of it's "Essence"
    //and play a copy of it for free (is played an extra time? or give it replay? or just is played?)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Essence),
    ]);

    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        // 1. Préparer les préférences
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1)
        {
            Cancelable = true, //todo probably not what i think
            RequireManualConfirmation = true,
        };
        // 2. Lancer la commande de sélection
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => c is FiveElementsCard f && IsValidCirculationTarget(f), 
            this
        );

        // 3. Vérifier si une carte a bien été choisie
        var selectedModel = selection?.FirstOrDefault();
    
        if (selectedModel is FiveElementsCard card)
        {
            // On boucle sur tous les tags de la carte choisie
            foreach (var tag in card.ElementTags)
            {
                // On ignore le Neutre, et on ajoute 1 essence pour chaque autre tag trouvé
                if (tag != CardElementTag.Neutral)
                {
                    if (CombatState != null) CombatState.GetElementalStatus().AddEssence(tag, 1);
                }
            }

            //todo circulation is currently played after the card selected maybe making cost free is a better option
            //on joue la carte gratos sur une target random
            var clone = card.CreateClone();
            await CardCmd.AutoPlay(choiceContext, clone, null,AutoPlayType.Default,false,true);
            clone.RemoveFromState(); //probleme card is played before circulation
        }
    }

    protected override void OnUpgrade()
    {

    }
}