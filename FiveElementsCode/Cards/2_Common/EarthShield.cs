using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class EarthShield() : EarthCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    //Gain 4 block for each enemy, Earth:(Next turn gain 4 block)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(4,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        // maybe add block only once and multiply instead??
        foreach (var hittableEnemy in CombatState.HittableEnemies)
        {
            await CommonActions.CardBlock(this, play);
        }
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await CommonActions.ApplySelf<BlockNextTurnPower>(this, DynamicVars.Block.BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
    }
}