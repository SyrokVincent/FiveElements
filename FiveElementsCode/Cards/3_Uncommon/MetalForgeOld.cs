using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class MetalForgeOld() : MetalCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy,false,false) //removed
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(Owner.Creature);
    
    // old //Deal 9 damage, Metal:(Upgrade a random card in the discard pile for each metal card played this turn)
    //
    // new // Gain 2(3) vigor, Metal:(Upgrade 2(3) random card in the discard pile.)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(9,ValueProp.Move),
        new CalculationBaseVar(0), // card upgraded
        new CalculationExtraVar(1),   
        new CalculatedVar("AmountOfMetalCardPlayedThisTurn").WithMultiplier((card, target) =>
            ElementHistoryUtils.CountPlayedCardsOfElement(card.CombatState, card.Owner, CardElementTag.Metal))
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
        
        if (CardElementTag.Metal.IsActive(Owner.Creature))
        {
            var amountOfMetalCardPlayedThisTurn = DynamicVars["AmountOfMetalCardPlayedThisTurn"].PreviewValue;
            
            // Logique d'amélioration des cartes dans la défausse
            // On récupère les cartes améliorables, on en selectione X au hasard selon l' RNG du combat
            var upgradableCards = PileType.Discard.GetPile(Owner).Cards
                .Where(c => c.IsUpgradable)
                .TakeRandom((int)amountOfMetalCardPlayedThisTurn, Owner.RunState.Rng.CombatCardSelection);

            foreach (var cardToUpgrade in upgradableCards)
            {
                CardCmd.Upgrade(cardToUpgrade);
                CardCmd.Preview(cardToUpgrade); // Affiche la petite animation/prévisualisation de la carte améliorée
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}