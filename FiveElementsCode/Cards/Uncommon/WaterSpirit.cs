using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards.Uncommon;

public sealed class WaterSpirit() : WaterCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{

    //Gain 1 energy the first 1 time you play a water card each turn (maybe upgrade to 2 and cost 3 ?)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1), //just for description
        new PowerVar<WaterSpiritPower>(1),
    ]);

    // overided to remove echo and water:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        this.EnergyHoverTip,
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<WaterSpiritPower>(this, DynamicVars["WaterSpiritPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        this.AddKeyword(CardKeyword.Innate);
    }
}