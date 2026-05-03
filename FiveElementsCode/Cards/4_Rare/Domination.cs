using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class Domination() : NeutralCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && HasValidTarget();

    protected override bool ShouldGlowRedInternal => !(CombatState != null && HasValidTarget());
    
    private bool HasValidTarget()
    {
        // On récupère la main via la méthode d'extension standard
        var hand = PileType.Hand.GetPile(Owner).Cards;

        // On cherche s'il existe une carte (autre que celle-ci) 
        // qui correspond à tes critères de "Domination"
        return hand.Any(c => c != this && IsValidDominationTarget(c));
    }

    //todo ?? can curently copy neutral card when you have spirits form
    private bool IsValidDominationTarget(CardModel card)
    {
        var currentEcho = Character.FiveElements.Echo;
        
        if (currentEcho.Contains(CardElementTag.Water) && card.CountAsElement(CardElementTag.Fire,Owner.Creature)) return true;
        if (currentEcho.Contains(CardElementTag.Wood) && card.CountAsElement(CardElementTag.Earth,Owner.Creature)) return true;
        if (currentEcho.Contains(CardElementTag.Fire) && card.CountAsElement(CardElementTag.Metal,Owner.Creature)) return true;
        if (currentEcho.Contains(CardElementTag.Earth) && card.CountAsElement(CardElementTag.Water,Owner.Creature)) return true;
        if (currentEcho.Contains(CardElementTag.Metal) && card.CountAsElement(CardElementTag.Wood,Owner.Creature)) return true;
        
        return false;
    }
    
    
    //Transform a card that Echo dominate, into the previous card and play it
    // removed exhaust
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        //CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Dominate),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        //HoverTipFactory.FromPower<WavePower>(),
    ]);
    
    private CardModel? _cardToPlay;
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        // 1. Préparer les préférences
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        // 2. Lancer la commande de sélection
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            IsValidDominationTarget, 
            this
        );
        var cardsToTransform = selection.FirstOrDefault();
        
        //Trouver la dernière carte jouée ce tour (en ignorant Domination elle-même)
        var lastPlayedCard = CombatManager.Instance.History.CardPlaysStarted
            .Where(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Card != this)
            .Select(e => e.CardPlay.Card)
            .LastOrDefault(); // On prend la plus récente
        
        //la transformer
        
        if (lastPlayedCard != null && cardsToTransform != null && CombatState != null)
        {
            var clonedCard = lastPlayedCard.CreateClone();
            await FiveElementsCardExtensions.TransformInHand(cardsToTransform, clonedCard, false,CombatState);
            _cardToPlay = clonedCard;
        }
        //la jouer gratos plus tard
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card == this && _cardToPlay != null)
        {
            //jouer la carte selectioné plus tot
            await CardCmd.AutoPlay(context, _cardToPlay, null,AutoPlayType.Default,false,false);
            _cardToPlay = null;
        }
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(FiveElementsKeywords.Attune);
    }
}