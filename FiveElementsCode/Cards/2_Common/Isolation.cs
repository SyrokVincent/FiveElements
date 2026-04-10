using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class Isolation() : NeutralCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    //Gain 6 block, Add 1 Elemental Fulu in hand
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6,ValueProp.Move),
        new CardsVar("Fulus", 1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Fulu>(IsUpgraded),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        if (CombatState != null) await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner, 1, IsUpgraded, CombatState);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}