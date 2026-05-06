using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class EarthFoundation() : EarthCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{

    public override bool GainsBlock => true;

    //delete if shouldn't glow or replace water
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(Owner.Creature);

    //Deal 4(6) Gain 4(6) Block, Earth: (Choose a card in Hand to retain this turn) 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(4,ValueProp.Move),
        new BlockVar(4,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        
        await CommonActions.CardAttack(this,play.Target).Execute(choiceContext);
        await CommonActions.CardBlock(this, play);
       
        if (CardElementTag.Earth.IsActive(Owner.Creature))
        {
            // select a card to give it singleturnretain
            // 1. Préparer les préférences
            CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);

            // 2. Lancer la commande de sélection
            var selection = await CardSelectCmd.FromHand(
                choiceContext, 
                Owner, 
                prefs, 
                c => !c.ShouldRetainThisTurn, 
                this
            );
            var selectedModel = selection?.FirstOrDefault();
            selectedModel?.GiveSingleTurnRetain();

        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}