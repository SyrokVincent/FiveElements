using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards.Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards.Uncommon;

public sealed class FireRise() : FireCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    
    //Every 3 time you apply burn add a Fire plume in hand. Increased by 1 every time it create one. (if upgraded also create one on play)
    //todo need something to block easy infinite spam, completely broken right now
    //(like instead of increasing nb of plus gained, increased the total amount of possible plume gain( can only trigger x times, or x time per turn)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<FireRisePower>(1), // nb of plume created
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("Threshold").WithMultiplier((card, target) =>
        {
            var power = card.Owner.Creature.GetPower<FireRisePower>();
            if (power != null) {
                return power.CurrentThreshold;
            }
            return 3;  //when changing base value here need to also change value in the power
           
        }),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromCard<FirePlume>(),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Incandescence),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        //add power to self
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<FireRisePower>(this, DynamicVars["FireRisePower"].BaseValue);
        if (IsUpgraded)
        {
            if (CombatState != null) await FiveElementsCard.CreateInHand<FirePlume>(Owner, 1, false, CombatState);
        }
    }

    protected override void OnUpgrade()
    {
        
    }
}