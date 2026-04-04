using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards.Common;

  
public sealed class WaterCreation() : WaterCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CardElementTag.Water.IsActive(CombatState);
    
    //Water: (1 energy 2 wave), Gain 1 "water element"
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1), 
        new PowerVar<WavePower>(2),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CardElementTag.Water.IsActive(CombatState))
        {
            await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
            await CommonActions.ApplySelf<WavePower>(this, DynamicVars["WavePower"].BaseValue);
        }

        if (CombatState != null) CombatState.GetElementalStatus().AddEssence(CardElementTag.Water, 1);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars["WavePower"].UpgradeValueBy(2);
    }
}