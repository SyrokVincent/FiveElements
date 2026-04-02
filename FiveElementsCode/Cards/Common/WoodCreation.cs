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

  
public sealed class WoodCreation() : WoodCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Wood.IsActive();
    
    //Wood:(draw 1), Gain 1 "Wood element"
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1), 
    ]);


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CardElementTag.Wood.IsActive())
        {
            await CommonActions.Draw(this, choiceContext);
        }
        WoodEnergy += 1;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}