using BaseLib.Utils;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._5_Token;

[Pool(typeof(TokenCardPool))]
public sealed class WoodFulu() : WoodCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    //Exhaust, Shift (Ethereal?) give 1 surge
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<SurgePower>(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Ethereal,
        FiveElementsKeywords.Shift,
        FiveElementsKeywords.Echo,
        FiveElementsKeywords.Generate,
        CardKeyword.Exhaust, 
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Shift),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromPower<SurgePower>(),
    ];
    
    //change element when a card is played
    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 1. On ne se transforme que si une AUTRE carte est jouée par le propriétaire
        if (cardPlay.Card == this || Owner != cardPlay.Card.Owner) return;
        
        await this.TryShiftFuluTransform();
    }  
    
    public override async Task AfterTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        await this.TryShiftFuluTransform();
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        //nothing to do ?
        await CommonActions.ApplySelf<SurgePower>(choiceContext,this, DynamicVars["SurgePower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        this.RemoveKeyword(CardKeyword.Ethereal);
        this.AddKeyword(CardKeyword.Retain);
    }
}