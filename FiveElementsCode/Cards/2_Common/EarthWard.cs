using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(Owner.Creature);

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
        if (CardElementTag.Earth.IsActive(Owner.Creature))
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

    public override async Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        await base.AfterFlush(choiceContext, player, flushedCards, retainedCards);
        
        if (retainedCards.Contains(this))
        {
            this.EnergyCost.AddUntilPlayed(-1);
            var decreaseAmount = DynamicVars["ReductionOnRetain"].BaseValue;
            DynamicVars.Block.BaseValue = Math.Max(0, (int)DynamicVars.Block.BaseValue - (int)decreaseAmount);
            ExtraBlockFromRetains -= decreaseAmount;
        }
    }
    
    
    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Block.BaseValue -= ExtraBlockFromRetains;
    }

}