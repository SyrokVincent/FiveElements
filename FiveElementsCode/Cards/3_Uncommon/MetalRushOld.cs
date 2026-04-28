using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class MetalRushOldVersion() : MetalCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self,false,false)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    // old // Return in hand and increase it's cost and it's vigor by 1 this turn, Metal:(Gain 1 vigor)
    //
    // new // Gain 2(3) vigor, Metal:(gain 1 vigor. Return in hand if it is the first card you play this turn.)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(1),
        new CalculationExtraVar(1),
        new CalculatedVar("VigorThisTurn").WithMultiplier((card, _) => 
            CombatManager.Instance.History.CardPlaysFinished.Count(e => 
                e.HappenedThisTurn(card.CombatState) && 
                e.CardPlay.Card == card && 
                e.CardPlay.Card.Owner == card.Owner)),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            await PowerCmd.Apply<VigorPower>(choiceContext,this.Owner.Creature, DynamicVars["VigorThisTurn"].PreviewValue, Owner.Creature, this,false);
        }
        this.EnergyCost.AddThisTurn(1);
        await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(1);
    }
}