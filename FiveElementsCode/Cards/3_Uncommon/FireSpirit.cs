using BaseLib.Utils;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;


public static class FireSpiritVars
{
    public static PowerVar<FireSpiritPower> FireSpirit => new PowerVar<FireSpiritPower>(2);
}

public sealed class FireSpirit() : FireCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{

    //Apply 2 burn to all enemies when you play a fire card 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        FireSpiritVars.FireSpirit, //pass this number because it need to be the same as the one on firespiritpower
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<BurnPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<FireSpiritPower>(choiceContext,this, DynamicVars["FireSpiritPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        this.AddKeyword(CardKeyword.Innate);
    }
}