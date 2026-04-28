using FiveElements.FiveElementsCode.Cards._5_Token;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class MoreOrLess() : NeutralCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{

    //Exhaust, choose between More or Less to add into your hand
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Exhaust,
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<More>(IsUpgraded),
        HoverTipFactory.FromCard<Less>(IsUpgraded),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        
        
        if (CombatState != null)
        {
            // 1. Créer les instances des cartes More et Less
            var moreCard = CombatState.CreateCard<More>(this.Owner);
            var lessCard = CombatState.CreateCard<Less>(this.Owner);

            // On les met dans une liste pour l'écran de sélection
            var choices = new List<CardModel> { moreCard, lessCard };
            if (IsUpgraded)
            {
                CardCmd.Upgrade(choices, CardPreviewStyle.HorizontalLayout);
            }

            // Écran de sélection (Discover)
            CardModel? selectedInternalCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, Owner, false);
        
            if (selectedInternalCard != null)
            {
                // Ajout de la carte générée dans la main avec animation
                await CardPileCmd.AddGeneratedCardToCombat(selectedInternalCard, PileType.Hand, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {

    }
}