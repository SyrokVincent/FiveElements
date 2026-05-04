using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class EarthWard() : EarthCard(1,
    CardType.Skill, CardRarity.Common,
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
    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    // old// Gain 7 block, Earth:(gain 2 temp Dex)
    //
    // new // :Retain, Reduce cost by 1 and block by 3(4) on retain, Earth:(Gain 7(10) block)
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1),
        new IntVar("ReductionOnRetain",3),
        new BlockVar(7,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Retain,
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(CardKeyword.Retain)
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await CommonActions.CardBlock(this, play);
        }
        ResetBlockValue();
    }

    private void ResetBlockValue()
    {
        DynamicVars.Block.BaseValue = IsUpgraded ? 10 : 7; 
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars["ReductionOnRetain"].UpgradeValueBy(1);
    }
    
    
    public override async Task AfterCardRetained(CardModel card)
    {
        if (card == this)
        {
            this.EnergyCost.AddUntilPlayed(-DynamicVars.Energy.IntValue);
            var decreaseAmount = DynamicVars["ReductionOnRetain"].BaseValue;
            // On réduit, mais on s'assure de ne pas descendre en dessous de 0
            DynamicVars.Block.BaseValue = Math.Max(0, DynamicVars.Block.BaseValue - decreaseAmount);
            ExtraBlockFromRetains -= decreaseAmount;
        }
        await Task.CompletedTask;
    }
    
    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Block.BaseValue -= ExtraBlockFromRetains;
    }

}