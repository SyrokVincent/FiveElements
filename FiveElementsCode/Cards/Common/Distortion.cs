using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards.Token;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Common;

public class Distortion() : NeutralCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    //Deal 7, Add 1 Elemental Fulu in hand
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7,ValueProp.Move),
        new CardsVar("Fulus", 1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Fulu>(IsUpgraded),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
        if (CombatState != null) await Fulu.CreateInHand(Owner, 1, IsUpgraded, CombatState);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}