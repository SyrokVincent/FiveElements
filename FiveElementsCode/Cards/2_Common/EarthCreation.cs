using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class EarthCreation() : EarthCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    protected override bool ShouldGlowGoldInternal => CardElementTag.Earth.IsActive(CombatState);

    //gain 5 Block,
    //Earth: (Gain 1 "earth element")
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(5, ValueProp.Move), 
    ]);


    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        await CommonActions.CardBlock(this, play);
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            CombatState.GetElementalStatus().AddEssence(CardElementTag.Earth, 1);
        }

    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars.Block.UpgradeValueBy(3);
    }
}