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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Common;

  
public sealed class EarthCreation() : EarthCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CardElementTag.Earth.IsActive(CombatState);

    //Earth: (gain 6 Block), Gain 1 "earth element"
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(6, ValueProp.Move), 
    ]);


    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await CommonActions.CardBlock(this, play);
        }

        if (CombatState != null) CombatState.GetElementalStatus().AddEssence(CardElementTag.Earth, 1);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars.Block.UpgradeValueBy(3);
    }
}