using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class FireTouch() : FireCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{

    //delete if shouldn't glow or replace water
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(CombatState);

    //Exhaust, Exhaust a (non-Fire?) card, Fire:(add 1 Fire plume in hand)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<FirePlume>(IsUpgraded),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Incandescence),
        HoverTipFactory.FromPower<BurnPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        /*
        // select of a non-fire card
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => !(c is FiveElementsCard f && f.IsFire()) && c != this,
            this
        );
        */
        
        // select of a card
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => c != this,
            this
        );
        
        
        var selectedCard = selection?.FirstOrDefault();
        if (selectedCard != null)
        {
            await CardCmd.Exhaust(choiceContext, selectedCard);
        }
        
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            await FiveElementsCardExtensions.CreateInHand<FirePlume>(Owner, DynamicVars.Cards.IntValue,this.IsUpgraded, CombatState);
        }
    }

    protected override void OnUpgrade()
    {
        //already create fire plume+
    }
}