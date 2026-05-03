using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class EarthGuardian() : EarthCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    
    //Gain 2 dex, 3 thorns, 4 plating
    // moved to uncommon, 2mana 2,3,4 to 1 mana 1,2,3
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<DexterityPower>(1),
        new PowerVar<ThornsPower>(2),
        new PowerVar<PlatingPower>(3),
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
        //DynamicVars["ThornsPower"].UpgradeValueBy(1);
        //DynamicVars["PlatingPower"].UpgradeValueBy(1);
    }
}