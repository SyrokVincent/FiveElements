using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class WaterMark() : WaterCard(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
{
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(CombatState);
    
    //Apply 1 weak to all enemies, Water:(next turn add water drop in hand)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<WeakPower>(1),
        new PowerVar<WaterDropNextTurnPower>(1),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<WaterDropNextTurnPower>(),
    ]);
    

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        foreach (var target in CombatState.HittableEnemies)
        {
            await CommonActions.Apply<WeakPower>(target, this, DynamicVars["WeakPower"].BaseValue);
        }
        if (CardElementTag.Water.IsActive(CombatState))
        {
            await CommonActions.ApplySelf<WaterDropNextTurnPower>(this, DynamicVars["WaterDropNextTurnPower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakPower"].UpgradeValueBy(1);
    }
    
}