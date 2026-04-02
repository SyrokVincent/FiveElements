using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards.Common;

  
public sealed class FireCreation() : FireCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AllEnemies)
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Fire.IsActive();
    
    //Fire: (Apply 4 Burn to all enemies), Gain 1 "fire element"
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<BurnPower>(4),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<BurnPower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CardElementTag.Fire.IsActive())
        {
            if (CombatState != null)
                foreach (var hittableEnemy in CombatState.HittableEnemies)
                {
                    await CommonActions.Apply<BurnPower>(hittableEnemy, this, DynamicVars["BurnPower"].BaseValue);
                }
        }
        FireEnergy += 1;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars["BurnPower"].UpgradeValueBy(2);
    }
}