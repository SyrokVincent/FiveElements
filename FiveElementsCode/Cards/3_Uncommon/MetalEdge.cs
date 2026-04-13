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

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class MetalEdge() : MetalCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    //Damage of the next attack increase by 50%, Metal:(next attack this turn cost 1 less (2? free?))
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<MetalEdgePower>(50), //value du %
        new PowerVar<MetalEdgeDiscountPower>(1), // value of discount
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<MetalEdgePower>(this, DynamicVars["MetalEdgePower"].BaseValue);
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            //next attack cost reduction
            //parcourir les carte de la main, reduire le cout des attack de 1 pour ce tour, lorsque une est joué enlever le buff des autre attack
            await CommonActions.ApplySelf<MetalEdgeDiscountPower>(this, DynamicVars["MetalEdgeDiscountPower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MetalEdgePower"].UpgradeValueBy(25);
    }
}