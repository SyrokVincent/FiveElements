using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class WoodBark() : WoodCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{

    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //Gain 7(+3) block,
    //Wood:(block of this card also scale with strength)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(7), // Base block
        new CalculationExtraVar(1),    // bonus block for each strength
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            var woodIsActive = card.CombatState != null && CardElementTag.Wood.IsActive(card.CombatState);

            if (!woodIsActive) 
                return 0;

            // On renvoie le multiplicateur (nombre de fois qu'on ajoute CalculationExtraVar)
            var strength = card.Owner.Creature.GetPowerAmount<StrengthPower>();
            return strength; //Math.Max(0, strength);  
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.CalculatedBlock.PreviewValue,ValueProp.Unpowered, play);
        
        //calculate prend pas en compte la dex ??
        //await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.CalculatedBlock.Calculate(play.Target),DynamicVars.CalculatedBlock.Props, play);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3);
    }
}