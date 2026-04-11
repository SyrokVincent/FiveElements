using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class WaterFlow() : WaterCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(CombatState);
    
    //Gain 2 energy, draw 1, Water:(next turn add water drop in hand)

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(2),
        new CardsVar(1),
        new PowerVar<WaterDropNextTurnPower>(1),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WaterDropNextTurnPower>(),
        HoverTipFactory.FromCard<WaterDrop>(),
    ]);


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
        await CommonActions.Draw(this, choiceContext);
        if (CardElementTag.Water.IsActive(CombatState))
        {
            await CommonActions.ApplySelf<WaterDropNextTurnPower>(this, DynamicVars["WaterDropNextTurnPower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}