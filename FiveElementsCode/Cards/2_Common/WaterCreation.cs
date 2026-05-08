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

  
public sealed class WaterCreation() : WaterCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self,false,false) //removed
{
    
    protected override bool ShouldGlowGoldInternal => CardElementTag.Water.IsActive(Owner.Creature);
    
    // Gain 1 energy and 2 wave
    // Water: (Gain 1 water essence)
    // removed innate on upgrade
    // test: now double the card effect and exhaust itself if you already have the essence
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1), 
        new PowerVar<WavePower>(2),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;

        if (Owner.Creature.GetElementalStatus().GetEssence(CardElementTag.Water) > 0)
        {
            await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue*2, Owner);
            await CommonActions.ApplySelf<WavePower>(choiceContext,this, DynamicVars["WavePower"].BaseValue*2);
        }
        else
        {
            await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
            await CommonActions.ApplySelf<WavePower>(choiceContext,this, DynamicVars["WavePower"].BaseValue);
            if (CardElementTag.Water.IsActive(Owner.Creature))
            {
                Owner.Creature.GetElementalStatus().AddEssence(CardElementTag.Water, 1,choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WavePower"].UpgradeValueBy(2);
    }
    
    //this shit is called before onplay
    //change the pile to exhaust if you have the corresponding essence
    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType resultPileType = base.GetResultPileTypeForCardPlay();
        return CombatState != null && (Owner.Creature.GetElementalStatus().GetEssence(CardElementTag.Water) > 0) ? PileType.Exhaust : resultPileType;
    }
    
}