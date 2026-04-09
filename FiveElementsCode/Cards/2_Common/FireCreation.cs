using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class FireCreation() : FireCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Fire.IsActive(CombatState);
    
    //Fire: (Apply 3 Burn to all enemies), Gain 1 "fire element"
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<BurnPower>(3),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<BurnPower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            if (CombatState != null)
                foreach (var hittableEnemy in CombatState.HittableEnemies)
                {
                    await CommonActions.Apply<BurnPower>(hittableEnemy, this, DynamicVars["BurnPower"].BaseValue);
                }
        }

        if (CombatState != null) CombatState.GetElementalStatus().AddEssence(CardElementTag.Fire, 1);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars["BurnPower"].UpgradeValueBy(2);
    }
    
    public override TargetType TargetType 
    {
        get
        {
            if (CardElementTag.Fire.IsActive(CombatState))
            {
                return TargetType.AllEnemies;
            }
            return  TargetType.Self;
        }
    } 
}