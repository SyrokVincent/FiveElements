using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class MetalMark() : MetalCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(Owner.Creature);

    
    
    // old // Apply 1 vulnerable, Metal:(Deal 6 damage(or gain vigor?))
    //
    // new // Apply 2(3) vulnerable, Metal:(Put a card from your Discard Pile on top of your Draw Pile)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VulnerablePower>(2),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VulnerablePower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target != null)
        {
            await CommonActions.Apply<VulnerablePower>(choiceContext, play.Target, this, DynamicVars["VulnerablePower"].BaseValue);
            if (CombatState != null && CardElementTag.Metal.IsActive(Owner.Creature))
            {
                // Sélection d'une carte dans la défausse
                CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        
                var discardPile = PileType.Discard.GetPile(Owner);
                var selection = await CardSelectCmd.FromSimpleGrid(choiceContext, discardPile.Cards, Owner, prefs);
        
                CardModel? selectedTrack = selection.FirstOrDefault();

                // Si une carte est choisie, on la place sur le dessus de la pioche
                if (selectedTrack != null)
                {
                    await CardPileCmd.Add(selectedTrack, PileType.Draw, CardPilePosition.Top);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VulnerablePower"].UpgradeValueBy(1);
    }
}