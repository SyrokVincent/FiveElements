using BaseLib.Extensions;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class MetalForge() : MetalCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    //todo mettre ça en calculatedvar
    //Deal 9 damage, Metal:(Upgrade a random card in the discard pile for each metal card played this turn)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(9,ValueProp.Move),
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
        
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            var amountOfMetalCardPlayedThisTurn = CombatManager.Instance.History.CardPlaysFinished.Count(e => 
            {
                if (!e.HappenedThisTurn(CombatState) || e.CardPlay.Card.Owner != Owner)
                    return false;
                
                // On récupère les tags figés au moment du jeu
                if (NeutralCard.PlayedElementsCache.TryGetValue(e.CardPlay, out var frozenTags))
                {
                    return frozenTags.TagsCountAsElement(CardElementTag.Metal, play.Card.Owner.Creature);
                }
                //si pas dans le cache, on utilise la méthode sur la carte
                return (e.CardPlay.Card.CountsAsElement(CardElementTag.Metal, play.Card.Owner.Creature));

            });
            
            
            // Logique d'amélioration des cartes dans la défausse
            // On récupère les cartes améliorables, on en selectione X au hasard selon l' RNG du combat
            var upgradableCards = PileType.Discard.GetPile(Owner).Cards
                .Where(c => c.IsUpgradable)
                .TakeRandom(amountOfMetalCardPlayedThisTurn, Owner.RunState.Rng.CombatCardSelection);

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