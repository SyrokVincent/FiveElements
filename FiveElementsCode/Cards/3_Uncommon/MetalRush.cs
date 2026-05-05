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

public sealed class MetalRush() : MetalCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    // old // Return in hand and increase it's cost and it's vigor by 1 this turn, Metal:(Gain 1 vigor)
    //
    // new // Gain 2(3) vigor, Metal:(gain 1 vigor. Return in hand if it is the first card you play this turn.)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VigorPower>(2),
        new PowerVar<VigorPower>("VigorPower2",1)
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
        await PowerCmd.Apply<VigorPower>(choiceContext,this.Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, this,false);
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            await PowerCmd.Apply<VigorPower>(choiceContext,this.Owner.Creature, DynamicVars["VigorPower2"].BaseValue, Owner.Creature, this,false);
            
            if (IsFirstManualCardPlayedThisTurn())
                await CardPileCmd.Add(this, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(1);
    }
    
    private bool IsFirstManualCardPlayedThisTurn()
    {
        if (CombatManager.Instance?.History == null || CombatState == null) return false;

        // On regarde l'historique des cartes terminées ce tour-ci
        // On ignore les cartes qui ont été jouées via un effet AutoPlay
        return !CombatManager.Instance.History.CardPlaysFinished.Any(e => 
                e.HappenedThisTurn(CombatState) &&
                e.CardPlay.Card.Owner == Owner && 
                !e.CardPlay.IsAutoPlay // <-- On ignore les Wood Surge et autres triggers
        );
    }
}