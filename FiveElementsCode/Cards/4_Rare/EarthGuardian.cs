using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public class EarthGuardian() : EarthCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    
    //Gain 2 dex, 3 thorns, 4 plating
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<DexterityPower>(2),
        new PowerVar<ThornsPower>(3),
        new PowerVar<PlatingPower>(4),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<ThornsPower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<DexterityPower>(choiceContext,this, DynamicVars["DexterityPower"].BaseValue);
        await CommonActions.ApplySelf<ThornsPower>(choiceContext,this, DynamicVars["ThornsPower"].BaseValue);
        await CommonActions.ApplySelf<PlatingPower>(choiceContext,this, DynamicVars["PlatingPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DexterityPower"].UpgradeValueBy(1);
        DynamicVars["ThornsPower"].UpgradeValueBy(1);
        DynamicVars["PlatingPower"].UpgradeValueBy(1);
    }
}