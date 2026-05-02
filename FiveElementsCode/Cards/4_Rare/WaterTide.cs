using BaseLib.Utils;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class WaterTide() : WaterCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    
    //At turn end gain 3 waves
    // upgraded to 3 from 2
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<WaterTidePower>(3),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WavePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<WaterTidePower>(choiceContext,this, DynamicVars["WaterTidePower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WaterTidePower"].UpgradeValueBy(1);
    }
}