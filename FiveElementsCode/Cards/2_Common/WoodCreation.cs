using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class WoodCreation() : WoodCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Wood.IsActive(CombatState);
    
    // Draw 1 and gain 1 temp strength
    // Wood:(Gain 1 "Wood essence")
    //removed innate on upgrade
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1), 
        new PowerVar<WoodCreationStrengthPower>(1)
    ]);


    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        await CommonActions.Draw(this, choiceContext);
        await CommonActions.ApplySelf<WoodCreationStrengthPower>(this, DynamicVars["WoodCreationStrengthPower"].BaseValue);
        if (CardElementTag.Wood.IsActive(this.CombatState))
        {
            CombatState.GetElementalStatus().AddEssence(CardElementTag.Wood, 1,choiceContext);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["WoodCreationStrengthPower"].UpgradeValueBy(1);
    }
}