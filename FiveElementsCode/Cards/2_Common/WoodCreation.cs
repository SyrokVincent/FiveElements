using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class WoodCreation() : WoodCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self,false,false) //removed
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Wood.IsActive(Owner.Creature);
    
    // Draw 1 and gain 1 surge
    // Wood:(Gain 1 "Wood essence")
    //removed innate on upgrade
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1), 
        new PowerVar<SurgePower>(1)
    ]);


    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);
    
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<SurgePower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        if (Owner.Creature.GetElementalStatus().GetEssence(CardElementTag.Wood) > 0)
        {
            await CardPileCmd.Draw(choiceContext, this.DynamicVars.Cards.BaseValue*2, this.Owner);
            await CommonActions.ApplySelf<SurgePower>(choiceContext,this, DynamicVars["SurgePower"].BaseValue*2);
        }
        else
        {
            await CommonActions.Draw(this, choiceContext);
            await CommonActions.ApplySelf<SurgePower>(choiceContext,this, DynamicVars["SurgePower"].BaseValue);
            if (CardElementTag.Wood.IsActive(Owner.Creature))
            {
                Owner.Creature.GetElementalStatus().AddEssence(CardElementTag.Wood, 1,choiceContext);
            }
        }
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SurgePower"].UpgradeValueBy(1);
    }
    
    //this shit is called before onplay
    //change the pile to exhaust if you have the corresponding essence
    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType resultPileType = base.GetResultPileTypeForCardPlay();
        return CombatState != null && (Owner.Creature.GetElementalStatus().GetEssence(CardElementTag.Wood) > 0) ? PileType.Exhaust : resultPileType;
    }
}