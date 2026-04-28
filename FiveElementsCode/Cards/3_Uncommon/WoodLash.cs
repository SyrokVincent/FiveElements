using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class WoodLash() : WoodCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{


    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //Deal 2 damage to a random enemy 4 time,
    //Wood:(Increase hit count by 1 this combat)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(2, ValueProp.Move),
        new RepeatVar(4),
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
        
        // Utilisation du builder d'attaque pour gérer les rebonds
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue) // Nombre de répétitions
            .FromCard(this)
            .TargetingRandomOpponents(CombatState)      // Cible des ennemis au hasard à chaque coup
            .WithHitFx("vfx/vfx_attack_slash")         // Effet visuel par coup
            .Execute(choiceContext);
        
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            // 2. Augmenter le nombre de hit de cette instance précise pour le reste du combat
            DynamicVars.Repeat.BaseValue += 1;
            ExtraRepeatFromPlays += 1;
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
    
    private decimal _extraRepeatFromPlays;
    
    // Propriété pour suivre l'augmentation cumulée.
    // Important : AssertMutable() assure que le changement ne se fait que sur une instance modifiable.
    private decimal ExtraRepeatFromPlays
    {
        get => _extraRepeatFromPlays;
        set
        {
            AssertMutable();
            _extraRepeatFromPlays = value;
        }
    }
    
    // Sécurité StS2 : Si la carte est "rétrogradée" (ex: effet d'ennemi), 
    // on s'assure que les repeat accumulés sont réappliqués sur la version de base.
    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Repeat.BaseValue += ExtraRepeatFromPlays;
    }
}