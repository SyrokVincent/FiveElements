using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class EarthWard() : EarthCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{

    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    //Gain 7 block, Earth:(gain 2 temp Dex)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(7,ValueProp.Move),
        new PowerVar<EarthWardPower>(2),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<DexterityPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;
        
        await CommonActions.CardBlock(this, play);
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            await CommonActions.ApplySelf<EarthWardPower>(this, DynamicVars["EarthWardPower"].BaseValue);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars["EarthWardPower"].UpgradeValueBy(1);
    }
}