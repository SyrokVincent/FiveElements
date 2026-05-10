using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Extensions;

namespace FiveElements.FiveElementsCode.Potions;

public sealed class CreationPotion :FiveElementsPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
    
    // Définition des variables dynamiques (Dégâts affichés dans la description)
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new CardsVar(1);
        }
    }
    
    public override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard<Creation>();
        }
    }
    
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        //todo some animation stolen from other potion    
        //ajout de la carte
        if (Owner.Creature.CombatState != null)
            await FiveElementsCardExtensions.CreateInHand<Creation>(Owner, 1, false, Owner.Creature);
    }
    
}

