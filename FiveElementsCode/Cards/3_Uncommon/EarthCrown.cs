using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class EarthCrown() : EarthCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{

    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    // old// Gain 4*2 block, Earth:(This turn for each Earth card played gain 1 temp dex)
    //
    // new// Gain 5 block, Earth:(Gain 2 block next turn, gains 2 additional block for every earth card played this turn)
    // moved to common from uncommon
    // moved back to uncommon, now grant plating for each earth card played before
    // new// Gain 8 block, Earth:(Gain 1 Plating for every earth card played this turn)

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(8,ValueProp.Move),
        new PowerVar<PlatingPower>(1),
        new CalculationBaseVar(0), // Base plating
        new CalculationExtraVar(1),    // bonus plating for each earth card
        new CalculatedVar("EarthCardPlayed").WithMultiplier((card, target) =>
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
        HoverTipFactory.FromPower<PlatingPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        await CommonActions.CardBlock(this, play);
        if (CardElementTag.Earth.IsActive(CombatState))
        {   
            await CommonActions.ApplySelf<PlatingPower>(choiceContext,this, DynamicVars["PlatingPower"].BaseValue * DynamicVars["EarthCardPlayed"].PreviewValue);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        //DynamicVars.CalculationBase.UpgradeValueBy(1);
        //DynamicVars.CalculationExtra.UpgradeValueBy(1);
        
    }
}