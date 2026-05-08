using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class EarthCreation() : EarthCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self,false,false) //removed
{
    
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    protected override bool ShouldGlowGoldInternal => CardElementTag.Earth.IsActive(Owner.Creature);

    //gain 5 Block,
    //Earth: (Gain 1 "earth element")
    //removed innate on upgrade
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
        
        if (Owner.Creature.GetElementalStatus().GetEssence(CardElementTag.Earth) > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue*2, DynamicVars.Block.Props, play);
        }
        else
        {
            await CommonActions.CardBlock(this, play);
            if (CardElementTag.Earth.IsActive(Owner.Creature))
            {
                Owner.Creature.GetElementalStatus().AddEssence(CardElementTag.Earth, 1,choiceContext);
            }
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
    
    //this shit is called before onplay
    //change the pile to exhaust if you have the corresponding essence
    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType resultPileType = base.GetResultPileTypeForCardPlay();
        return CombatState != null && (Owner.Creature.GetElementalStatus().GetEssence(CardElementTag.Earth) > 0) ? PileType.Exhaust : resultPileType;
    }
}