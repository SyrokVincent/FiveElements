using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Common;

  
public class EarthCreation() : EarthCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(6, ValueProp.Move), 
    ]);
//Earth: (gain 6 Block), Gain 1 "earth element"

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        StaticHoverTip("FIVEELEMENTS-ECHO",CanonicalVars),
        StaticHoverTip("FIVEELEMENTS-EARTH",CanonicalVars),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (IsElementActive(CardElementTag.Earth))
        {
            await CommonActions.CardBlock(this, play);
        }
        EarthEnergy += 1;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars.Block.UpgradeValueBy(3);
    }
}