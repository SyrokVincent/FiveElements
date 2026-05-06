using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class FireEater() : FireCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(Owner.Creature);

    //Ethereal, Exhaust 2(3) Fire card, Fire:(for each card exhausted apply 8 burn to all enemies)
    // buffed from 4 burn to 8
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(2),
        new PowerVar<BurnPower>(8),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Ethereal,
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, this.DynamicVars.Cards.IntValue);
        // select of a fire card
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => (c.CountAsElement(CardElementTag.Fire,Owner.Creature)) && c != this,
            this
        );

        var cardModels = selection.ToList();
        foreach (var card in cardModels)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
        
        // 3. Si l'élément FEU est actif, on applique Burn par carte épuisée
        if (CardElementTag.Fire.IsActive(Owner.Creature) && cardModels.Any())
        {
            // On répète l'action pour chaque carte épuisée
            for (var i = 0; i < cardModels.Count; i++)
            {
                if (CombatState != null)
                {
                    var targets = CombatState.HittableEnemies;
                    await PowerCmd.Apply<BurnPower>(choiceContext, targets, this.DynamicVars["BurnPower"].BaseValue, this.Owner.Creature, this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}