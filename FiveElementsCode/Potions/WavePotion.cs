using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Godot;
using BaseLib.Extensions;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Extensions;

namespace FiveElements.FiveElementsCode.Potions;

public sealed class WavePotion :FiveElementsPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyAlly;
    
    // Définition des variables dynamiques (Dégâts affichés dans la description)
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        { 
            yield return new PowerVar<WavePower>(8);
        }
    }
    
    public override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<WavePower>();
        }
    }
    
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        //todo some animation stolen from other potion    
        if (target != null) await CommonActions.Apply<WavePower>(choiceContext, target, null, DynamicVars["WavePower"].BaseValue);
    }
    
    
}

