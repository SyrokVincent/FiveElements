using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class WaterCreation() : WaterCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CardElementTag.Water.IsActive(CombatState);
    
    // Gain 1 energy and 2 wave
    // Water: (Gain 1 water essence)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1), 
        new PowerVar<WavePower>(2),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
        await CommonActions.ApplySelf<WavePower>(this, DynamicVars["WavePower"].BaseValue);
        if (CardElementTag.Water.IsActive(CombatState))
        {
            CombatState.GetElementalStatus().AddEssence(CardElementTag.Water, 1,choiceContext);
        }

    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars["WavePower"].UpgradeValueBy(2);
    }
}