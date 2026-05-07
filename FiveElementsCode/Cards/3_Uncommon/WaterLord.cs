using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;
public static class WaterLordVars
{
    public static PowerVar<WaterLordPower> WaterLord => new PowerVar<WaterLordPower>(2);
}
public sealed class WaterLord() : WaterCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(Owner.Creature);

    //Gain 4 wave, Water:(This turn for each water card played gain 2 Wave)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<WavePower>(4),
        WaterLordVars.WaterLord,
    ]);
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        await CommonActions.ApplySelf<WavePower>(choiceContext,this, DynamicVars["WavePower"].BaseValue);
        if (CardElementTag.Water.IsActive(Owner.Creature))
        {
            await CommonActions.ApplySelf<WaterLordPower>(choiceContext,this, DynamicVars["WaterLordPower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WavePower"].UpgradeValueBy(1);
        DynamicVars["WaterLordPower"].UpgradeValueBy(1);
    }
}