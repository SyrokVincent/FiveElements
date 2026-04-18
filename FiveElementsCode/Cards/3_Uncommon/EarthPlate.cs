using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class EarthPlate() : EarthCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{

    private decimal _extraBlockFromRetains;
    
    private decimal ExtraBlockFromRetains
    {
        get => _extraBlockFromRetains;
        set
        {
            AssertMutable();
            _extraBlockFromRetains = value;
        }
    }

    public override bool ShouldReceiveCombatHooks => true;
    
    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    //Retain, Gain 1 block, for each turn in hand increase by 2, Earth:(Gain 1 Plating for each enemy that plan to attack)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(1,ValueProp.Move),
        new IntVar("BlockIncrease",2),
        new PowerVar<PlatingPower>(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Retain, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<PlatingPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;
 
        await CommonActions.CardBlock(this, play);
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            
            // Check how many enemy intends to attack
            var enemyWithAttackIntent = 0;
            foreach (var enemy in CombatState.Enemies)
            {
                if (enemy.Monster != null && enemy.Monster.IntendsToAttack)
                {
                    enemyWithAttackIntent++;
                }
            }
            if (enemyWithAttackIntent>0)
            {
                await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
                await CommonActions.ApplySelf<PlatingPower>(this, DynamicVars["PlatingPower"].BaseValue*enemyWithAttackIntent);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["BlockIncrease"].UpgradeValueBy(1);
    }
    
    public override async Task AfterCardRetained(CardModel card)
    {
        if (card == this)
        {
            var increaseAmount = DynamicVars["BlockIncrease"].BaseValue;

            DynamicVars.Block.BaseValue += increaseAmount;
            ExtraBlockFromRetains += increaseAmount;
        }

        await Task.CompletedTask;
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Block.BaseValue += ExtraBlockFromRetains;
    }
}