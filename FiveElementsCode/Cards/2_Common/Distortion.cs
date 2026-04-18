using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class Distortion() : NeutralCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    // need a few card with strike tag
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    
    //Deal 9, Add 1 Fulu in hand
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9,ValueProp.Move),
        new CardsVar(1)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Shift,
    ]);
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Fulu>(IsUpgraded),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
        if (CombatState != null) await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner, DynamicVars.Cards.IntValue, false, CombatState);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}