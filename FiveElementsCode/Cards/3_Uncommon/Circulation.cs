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

public class Circulation() : NeutralCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    
    //protected override bool IsPlayable => CombatState != null && HasValidTarget();

    protected override bool ShouldGlowGoldInternal => CombatState != null && HasValidTarget();

    protected override bool ShouldGlowRedInternal => !(CombatState != null && HasValidTarget());
    
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
            if (currentEcho.Contains(CardElementTag.Water) && card.CountAsElement(CardElementTag.Wood,Owner.Creature)) return true;
            if (currentEcho.Contains(CardElementTag.Wood) && card.CountAsElement(CardElementTag.Fire,Owner.Creature)) return true;
            if (currentEcho.Contains(CardElementTag.Fire) && card.CountAsElement(CardElementTag.Earth,Owner.Creature)) return true;
            if (currentEcho.Contains(CardElementTag.Earth) && card.CountAsElement(CardElementTag.Metal,Owner.Creature)) return true;
            if (currentEcho.Contains(CardElementTag.Metal) && card.CountAsElement(CardElementTag.Water,Owner.Creature)) return true;
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

    
    
    private CardModel? _cardToPlay;
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        // 1. Préparer les préférences
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1)
        {
            //Cancelable = true, //todo probably not what i think
            RequireManualConfirmation = false,
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
            
            _cardToPlay = card;
        }
    }


    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        
        if (cardPlay.Card == this && _cardToPlay != null)
        {
            //jouer la carte selectioné plus tot
            await CardCmd.AutoPlay(context, _cardToPlay, null,AutoPlayType.Default,false,false);
            //_clone.RemoveFromState(); 
        }
    }

    protected override void OnUpgrade()
    {
        this.AddKeyword(FiveElementsKeywords.Attune);
    }
}