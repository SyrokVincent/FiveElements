using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class EarthCrown() : EarthCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{

    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    // old// Gain 4*2 block, Earth:(This turn for each Earth card played gain 1 temp dex)
    //
    // new// Gain 5 block, Earth:(Gain 2 block next turn, gains 2 additional block for every earth card played this turn)
    // moved to common from uncommon
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(5,ValueProp.Move),
        new CalculationBaseVar(2), // Base block
        new CalculationExtraVar(2),    // bonus block for each earth card
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            if (card.CombatState == null) return 0;

            return CombatManager.Instance.History.CardPlaysFinished.Count(e => 
            {
                if (!e.HappenedThisTurn(card.CombatState) || e.CardPlay.Card.Owner != card.Owner)
                    return false;
                
                // On récupère les tags figés au moment du jeu
                if (NeutralCard.PlayedElementsCache.TryGetValue(e.CardPlay, out var frozenTags))
                {
                    return frozenTags.TagsCountAsElement(CardElementTag.Earth, card.Owner.Creature);
                }
                //si pas dans le cache, on utilise la méthode sur la carte
                return (e.CardPlay.Card.CountAsElement(CardElementTag.Earth, card.Owner.Creature));
            });
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        await CommonActions.CardBlock(this, play);
        if (CardElementTag.Earth.IsActive(CombatState))
        {   
            await CommonActions.ApplySelf<BlockNextTurnPower>(this, DynamicVars.CalculatedBlock.PreviewValue);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars.CalculationBase.UpgradeValueBy(1);
        DynamicVars.CalculationExtra.UpgradeValueBy(1);
        
    }
}