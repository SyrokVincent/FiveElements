using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class FireCreation() : FireCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AllEnemies,false,false) //removed
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Fire.IsActive(CombatState);
    
    //Apply 3 Burn to all enemies
    //Fire: (Gain 1 fire essence)
    //removed innate on upgrade
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<BurnPower>(3),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<BurnPower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        if (CombatState.GetElementalStatus().GetEssence(CardElementTag.Fire) > 0)
        {
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<BurnPower>(choiceContext, targets, this.DynamicVars["BurnPower"].BaseValue*2, this.Owner.Creature, this);
        }
        else
        {
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<BurnPower>(choiceContext, targets, this.DynamicVars["BurnPower"].BaseValue, this.Owner.Creature, this);
    
            if (CardElementTag.Fire.IsActive(CombatState))
            {
                CombatState.GetElementalStatus().AddEssence(CardElementTag.Fire, 1,choiceContext);
            }
        }
        

    }

    protected override void OnUpgrade()
    {
        DynamicVars["BurnPower"].UpgradeValueBy(2);
    }
    
    //this shit is called before onplay
    //change the pile to exhaust if you have the corresponding essence
    protected override PileType GetResultPileType()
    {
        PileType resultPileType = base.GetResultPileType();
        return CombatState != null && (CombatState.GetElementalStatus().GetEssence(CardElementTag.Fire) > 0) ? PileType.Exhaust : resultPileType;
    }
    
}