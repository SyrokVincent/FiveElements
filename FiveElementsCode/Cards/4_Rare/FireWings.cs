using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class FireWings() : FireCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    
    //At turn start add 1 Fire plume in hand. (if upgraded also create one on play)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<FireWingsPower>(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<FirePlume>(),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Incandescence),
        HoverTipFactory.FromPower<BurnPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        //add power to self
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<FireWingsPower>(choiceContext,this, DynamicVars["FireWingsPower"].BaseValue);

        if (IsUpgraded)
        {
            if (CombatState != null) await FiveElementsCardExtensions.CreateInHand<FirePlume>(Owner, 1, false, CombatState);
        }
        
    }

    protected override void OnUpgrade()
    {

    }
}