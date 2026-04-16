using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class FireCard : FiveElementsCard, IOnFireStateChanged
{
    
    //public override Material? CreateCustomFrameMaterial => ShaderUtils.GenerateHsv(1.04f, 1.2f, 1.1f);
    
    protected FireCard(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target)
    {
        CanonicalElementTags = [CardElementTag.Fire];
    }   
    
    public async Task OnFireStateChanged(bool isActive)
    {
        DynamicVars["isFireOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isFireOn"),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    //need to overide for card that don't have Element:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fire),
    ]);
    
    
    protected async Task<int> DealHeatDamage(PlayerChoiceContext choiceContext, Creature? target, CalculatedDamageVar damage)
    {
        ArgumentNullException.ThrowIfNull(target);

        // Exécution de l'attaque
        var attackResult = await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // Calcul des dégâts non bloqués
        int unblockedDamage = attackResult.Results.Sum(r => r.UnblockedDamage);

        // Application du Burn si dégâts > 0
        if (unblockedDamage > 0)
        {
            await CommonActions.Apply<BurnPower>(target, this, unblockedDamage);
        }

        return unblockedDamage; // On retourne la valeur au cas où la carte en ait besoin pour autre chose
    }
    
    protected async Task<int> DealHeatDamage(PlayerChoiceContext choiceContext, Creature? target, DamageVar damageVar)
    {
        // Sécurité : Vérifie que la cible existe
        ArgumentNullException.ThrowIfNull(target);

        // Exécution de l'attaque via le DamageCmd
        var attackResult = await DamageCmd.Attack(damageVar.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // Calcul des dégâts non bloqués
        int unblockedDamage = attackResult.Results.Sum(r => r.UnblockedDamage);

        // Application du Burn si dégâts > 0
        if (unblockedDamage > 0)
        {
            await CommonActions.Apply<BurnPower>(target, this, unblockedDamage);
        }

        return unblockedDamage; // On retourne la valeur au cas où la carte en ait besoin pour autre chose
    }
}