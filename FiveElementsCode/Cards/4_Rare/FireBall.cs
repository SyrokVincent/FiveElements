using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class FireBall() : FireCard(10,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(Owner.Creature);

    //Fireboost, Deal 15 Heat damage to all enemies, Fire:(2 more for each other fire card in hand) 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(15), // Base damage
        new ExtraDamageVar(2),    // bonus damage for each fire card in hand
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            var fireIsActive = card.CombatState != null && CardElementTag.Fire.IsActive(card.Owner.Creature);

            if (!fireIsActive) 
                return 0;
            
            // Utilisation de la méthode d'extension StS2 pour récupérer la main
            var handCards = PileType.Hand.GetPile(card.Owner).Cards;

            // On compte les cartes Feu (en excluant la carte elle-même)
            int fireCardsInHand = handCards.Count(c => 
                c.CountAsElement(CardElementTag.Fire,card.Owner.Creature) &&
                c != card
            );

            // On renvoie le multiplicateur (nombre de fois qu'on ajoute ExtraDamageVar)
            return (decimal)fireCardsInHand;
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fireboost),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Heat),
        HoverTipFactory.FromPower<BurnPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
       
        await DealHeatDamageAoe(choiceContext, DynamicVars.CalculatedDamage);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3);
        DynamicVars.ExtraDamage.UpgradeValueBy(1);
    }
    /*
    //fireboost logic 1
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        
        //Only trigger if the owner of this card play a card
        if (Owner != cardPlay.Card.Owner) 
            return;
        
        //count of all fire card that don't exhaust
        // Check if the played card is a fire element card and if it's not going to exhaust
        if (cardPlay.Card is FiveElementsCard elementCard && elementCard.IsFire() && cardPlay.ResultPile != PileType.Exhaust)
        {
            this.EnergyCost.AddUntilPlayed(-1);
        }
        await Task.CompletedTask;
    }*/
    
    
    // Fireboost Logic 1 : Réduction par les cartes Feu jouées
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // Sécurité : Uniquement si le propriétaire joue la carte et que ce n'est pas celle-ci
        if (Owner != cardPlay.Card.Owner || cardPlay.Card == this) 
            return;

        // On vérifie si la carte jouée était du Feu au moment de son exécution
        bool wasFire = false;
    
        // On regarde dans le cache qu'on a mis en place pour le Shift/Attune
        if (NeutralCard.PlayedElementsCache.TryGetValue(cardPlay, out var frozenTags))
        {
            // On utilise l'extension de tags pour inclure SpiritsForm
            wasFire = frozenTags.TagsCountAsElement(CardElementTag.Fire, Owner.Creature);
        }
        else
        {
            // Fallback pour les cartes classiques
            wasFire = cardPlay.Card.CountAsElement(CardElementTag.Fire, Owner.Creature);
        }

        // Si c'était du feu ET que la carte ne s'épuise pas
        if (wasFire && cardPlay.ResultPile != PileType.Exhaust)
        {
            this.EnergyCost.AddUntilPlayed(-1);
        }
        await Task.CompletedTask;
    }
    
    
    //fireboost logic 2
    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        //Only trigger if the owner of this card exhaust a card
        if (Owner != card.Owner) 
            return;
        // count of all exhausted card 
        this.EnergyCost.AddUntilPlayed(-1);
        
        await Task.CompletedTask;
    }
    
    

    
}