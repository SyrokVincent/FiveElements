using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class WoodClaws() : WoodCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //Deal 7 damage,
    //Wood:(replay for each other wood card played this turn) 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(7,ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ReplayCount").WithMultiplier((card, target) =>
        {
            if (card.CombatState == null) return 0;
            
            return CombatManager.Instance.History.CardPlaysFinished.Count(e => 
            {
                if (!e.HappenedThisTurn(card.CombatState) || e.CardPlay.Card.Owner != card.Owner)
                    return false;
                // On récupère les tags figés au moment du jeu
                if (NeutralCard.PlayedElementsCache.TryGetValue(e.CardPlay, out var frozenTags))
                {
                    return frozenTags.TagsCountAsElement(CardElementTag.Wood, card.Owner.Creature);
                }
                //si pas dans le cache, on utilise la méthode sur la carte
                return (e.CardPlay.Card.CountsAsElement(CardElementTag.Wood, card.Owner.Creature));
            });
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
    ]);
    

    // Une simple variable suffit car l'AutoPlay réutilise cette instance d'objet
    private decimal _remainingReplays = 0;
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card != this) return;
        
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            if (!cardPlay.IsAutoPlay) 
            {
                // Premier passage : on initialise le compteur avec le nombre de carte bois joué
                _remainingReplays = DynamicVars["ReplayCount"].PreviewValue;
            }

            // Si on a encore des munitions, on continue la chaîne
            if (_remainingReplays > 0)
            {
                _remainingReplays--; // On décrémente avant de relancer
                await CardCmd.AutoPlay(context, this, cardPlay.Target);
            }
        }
    }


    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}