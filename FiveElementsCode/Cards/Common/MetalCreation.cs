using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards.Common;

  
public sealed class MetalCreation() : MetalCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Metal.IsActive();
    //Metal: (gain 3 vigor), Gain 1 "metal element"
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VigorPower>(3),
        new BoolVar("testelem",CardElementTag.Metal.IsActive())
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CardElementTag.Metal.IsActive())
        {
            await CommonActions.ApplySelf<VigorPower>(this, DynamicVars["VigorPower"].BaseValue);
        }
        MetalEnergy += 1;
    }

    protected override void OnUpgrade()
    {this.
        AddKeyword(CardKeyword.Innate);
        DynamicVars["VigorPower"].UpgradeValueBy(2);
    }
}