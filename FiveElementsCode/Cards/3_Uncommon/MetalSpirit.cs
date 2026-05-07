using BaseLib.Utils;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;
public static class MetalSpiritVars
{
    public static PowerVar<MetalSpiritPower> MetalSpirit => new PowerVar<MetalSpiritPower>(2);
}
public sealed class MetalSpirit() : MetalCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    
    // Gain 2 vigor when you play a metal card
    // buffed to 2
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        MetalSpiritVars.MetalSpirit, //need to be the same number as on metalspiritPower
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        
    ]);
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<MetalSpiritPower>(choiceContext,this, DynamicVars["MetalSpiritPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        this.AddKeyword(CardKeyword.Innate);
    }
}