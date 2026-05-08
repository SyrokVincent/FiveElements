using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class WaterCanon() : WaterCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy), IOnMetalStateChanged
{

    //delete if shouldn't glow or replace water
    protected override bool ShouldGlowGoldInternal => 
        CombatState != null && 
        (CardElementTag.Water.IsActive(Owner.Creature) || CardElementTag.Metal.IsActive(Owner.Creature));

    //Metal:(Deal 5 damage, gain wave equal to damage dealt),
    //Water:(Double wave until next turn start)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isMetalOn"),
        new DamageVar(5,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Metal),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Water),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromPower<WavePower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        if (CardElementTag.Metal.IsActive(Owner.Creature))
        {
            //ArgumentNullException.ThrowIfNull(play.Target, nameof(play.Target));

            // Exécute l'attaque et récupère les dégâts totaux (blocked + unblocked)
            var attackResult = await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
            decimal totalDamage = attackResult.Results.SelectMany(r => r).Sum(r => r.TotalDamage);

            // Gain wave based on damge dealt
            await CommonActions.ApplySelf<WavePower>(choiceContext,this, totalDamage);
        }
        if (CardElementTag.Water.IsActive(Owner.Creature))
        {
            // 1. On récupère le montant actuel de Wave
            var currentWave = play.Card.Owner.Creature.GetPowerAmount<WavePower>();

            if (currentWave > 0)
            {
                // 2. On ajoute le même montant pour "Doubler"
                await CommonActions.ApplySelf<WavePower>(choiceContext,this, currentWave);
                
                // 3. On applique un pouvoir négatif qui retirera ce surplus au tour suivant
                await CommonActions.ApplySelf<WaterCanonPower>(choiceContext,this, currentWave);
            }
      
        }
    }
    
    

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
    
    public override TargetType TargetType 
    {
        get
        {
            // 1. Protection indispensable pour la bibliothèque
            if (IsCanonical || Owner?.Creature == null)
            {
                return TargetType.Self;
            }
            if (CardElementTag.Metal.IsActive(Owner.Creature))
            {
                return TargetType.AnyEnemy;
            }
            return  TargetType.Self;
        }
    } 
    
    
    public async Task OnMetalStateChanged(bool isActive, Creature creature)
    {
        if (Owner.Creature != creature) return;
        DynamicVars["isMetalOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }

    public async Task OnElementStateChanged(CardElementTag element, bool isActive, Creature creature)
    {
        if (Owner.Creature != creature) return;
        if (element == CardElementTag.Water) await OnWaterStateChanged(isActive, creature);
        if (element == CardElementTag.Metal) await OnMetalStateChanged(isActive, creature);
    }
}