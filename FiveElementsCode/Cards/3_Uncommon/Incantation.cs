using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class Incantation() : NeutralCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    //At turn start add 1 Elemental Fulu in hand
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<IncantationPower>(1),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Fulu>(),
    ];
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        //add power to self
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<IncantationPower>(this, DynamicVars["IncantationPower"].BaseValue);

    }

    protected override void OnUpgrade()
    {
        this.EnergyCost.UpgradeBy(-1);
    }
}