using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class MetalForge() : MetalCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(Owner.Creature);
    
    // old //Deal 9 damage, Metal:(Upgrade a random card in the discard pile for each metal card played this turn)
    //
    // new // Gain 2(3) vigor, Return in hand and increase it's cost by 1 this turn, Metal:(Upgrade 2(3) random card in the discard pile.)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VigorPower>(2),
        new DynamicVar("UpgradeAmount",2),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        await PowerCmd.Apply<VigorPower>(choiceContext,this.Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, this,false);
        
        if (CardElementTag.Metal.IsActive(Owner.Creature))
        {
            // Logique d'amélioration des cartes dans la défausse
            // On récupère les cartes améliorables, on en selectione X au hasard selon l' RNG du combat
            var upgradableCards = PileType.Discard.GetPile(Owner).Cards
                .Where(c => c.IsUpgradable)
                .TakeRandom(DynamicVars["UpgradeAmount"].IntValue, Owner.RunState.Rng.CombatCardSelection);

            foreach (var cardToUpgrade in upgradableCards)
            {
                CardCmd.Upgrade(cardToUpgrade);
                CardCmd.Preview(cardToUpgrade); // Affiche la petite animation/prévisualisation de la carte améliorée
            }
        }
        
        //increase cost by 1 and return in hand
        this.EnergyCost.AddThisTurn(1);
        await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(1);
        DynamicVars["UpgradeAmount"].UpgradeValueBy(1);
    }
}