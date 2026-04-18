using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class WoodSeed() : WoodCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //Gain 2 Strength this turn,
    //Wood:(draw 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<WoodSeedPower>(2),
        new CardsVar(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<StrengthPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        //await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<WoodSeedPower>(this, DynamicVars["WoodSeedPower"].BaseValue);
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            await CommonActions.Draw(this, choiceContext);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["WoodSeedPower"].UpgradeValueBy(1);
    }
}