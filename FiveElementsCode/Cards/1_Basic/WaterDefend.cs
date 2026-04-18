using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._1_Basic;

public sealed class WaterDefend() : WaterCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(5, ValueProp.Move)
    ]);
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(3);
    }
}