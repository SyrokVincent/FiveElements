using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class Absorption() : NeutralCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    
    //Exhaust, consume all "Essence", gain 1 energy for each
    // no longer consume essence
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("EnergyGained").WithMultiplier((card, target) =>
        {
            if (card.CombatState != null) return card.CombatState.GetElementalStatus().GetTotalEssenceCount();
            return 0;
        }),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Essence),
        this.EnergyHoverTip,
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        if (CombatState != null)
        {
            var essencesCount = CombatState.GetElementalStatus().GetTotalEssenceCount();
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue * essencesCount, Owner);
            //CombatState.GetElementalStatus().ResetAllEssences();
        }
    }

    protected override void OnUpgrade()
    {
        this.AddKeyword(FiveElementsKeywords.Attune);
    }
}