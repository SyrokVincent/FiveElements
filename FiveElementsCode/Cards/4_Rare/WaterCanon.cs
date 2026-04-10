using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class WaterCanon() : WaterCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.Self), IOnMetalStateChanged
{

    //delete if shouldn't glow or replace water
    protected override bool ShouldGlowGoldInternal => 
        CombatState != null && 
        (CardElementTag.Water.IsActive(CombatState) || CardElementTag.Metal.IsActive(CombatState));

    //Metal:(Deal 3 damage, gain wave equal to damage dealt),
    //Water:(Double wave until next turn start)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isMetalOn"),
        new DamageVar(3,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Metal),
        HoverTipFactory.FromPower<WavePower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            //ArgumentNullException.ThrowIfNull(play.Target, nameof(play.Target));

            // Exécute l'attaque et récupère les dégâts totaux (blocked + unblocked)
            var attackResult = await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
            decimal totalDamage = attackResult.Results.Sum(r => r.TotalDamage);

            // Gain wave based on damge dealt
            await CommonActions.ApplySelf<WavePower>(this, totalDamage);
        }
        if (CardElementTag.Water.IsActive(CombatState))
        {
            // 1. On récupère le montant actuel de Wave
            var currentWave = play.Card.Owner.Creature.GetPowerAmount<WavePower>();

            if (currentWave > 0)
            {
                // 2. On ajoute le même montant pour "Doubler"
                await CommonActions.ApplySelf<WavePower>(this, currentWave);
                
                // 3. On applique un pouvoir négatif qui retirera ce surplus au tour suivant
                await CommonActions.ApplySelf<WaterCanonPower>(this, currentWave);
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
            if (CardElementTag.Metal.IsActive(CombatState))
            {
                return TargetType.AnyEnemy;
            }
            return  TargetType.Self;
        }
    } 
    
    
    public async Task OnMetalStateChanged(bool isActive)
    {
        DynamicVars["isMetalOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }

    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        if (element == CardElementTag.Water) await OnWaterStateChanged(isActive);
        if (element == CardElementTag.Metal) await OnMetalStateChanged(isActive);
    }
}