using BaseLib.Utils;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class EarthRelic() : FiveElementsRelic
{
    
    //At turn end gain 2 block for each earth card in hand
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(2,ValueProp.Unpowered), 
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.Static(StaticHoverTip.Block),
    ]);

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // On vérifie que c'est bien la fin du tour du joueur
        if (side != Owner.Creature.Side)
            return;

        // On filtre la main pour ne garder que les cartes avec l'élément Earth
        var earthCardsInHand = PileType.Hand.GetPile(Owner).Cards
            .Where(card => card.CountAsElement(CardElementTag.Earth,Owner.Creature))
            .ToList();

        if (earthCardsInHand.Count == 0)
            return;

        // Calcul du bloc basé uniquement sur les cartes Earth
        int blockAmount = (int)(earthCardsInHand.Count * DynamicVars.Block.BaseValue);

        this.Flash();

        // Application du bloc
        await CreatureCmd.GainBlock(
            Owner.Creature, 
            blockAmount, 
            ValueProp.Unpowered, 
            null
        );
    }
}
