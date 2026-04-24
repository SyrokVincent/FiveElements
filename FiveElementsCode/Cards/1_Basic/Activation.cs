using BaseLib.Abstracts;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._6_Ancient;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._1_Basic;


public static class ActivationVars
{
    // On définit les variables réutilisables pour les differente carte qui parle de Activation ici
    
    public static EnergyVar Energy => new EnergyVar(1);
    public static PowerVar<WavePower> Wave => new PowerVar<WavePower>(3);
    public static CardsVar Cards => new CardsVar(1);
    public static PowerVar<ActivationTempStrengthPower> TempStrength => new PowerVar<ActivationTempStrengthPower>(1);
    public static PowerVar<BurnPower> Burn => new PowerVar<BurnPower>(3);
    public static BlockVar Block => new BlockVar(5, ValueProp.Move);
    public static PowerVar<VigorPower> Vigor => new PowerVar<VigorPower>(2);
}


public sealed class Activation() : NeutralCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self), IOnElementStateChanged, ITranscendenceCard
{
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    protected override bool ShouldGlowGoldInternal => CombatState != null && FiveElementsCardExtensions.IsAnyElementActive(CombatState);
    
    // Water:(1 energy, 3 wave),
    // Wood:(Draw 1 and 1 temp str),
    // Fire:(Burn 3 to all enemies),
    // Earth:(5 block),
    // Metal:(2 vigor) 
    //
    // added 1 temp str and 1 wave
    //
    //VALUE HERE need to be the same as on Ultimate form that why i use Activationvars
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        
        //water
        ActivationVars.Energy,
        ActivationVars.Wave,
        //wood
        ActivationVars.Cards,
        ActivationVars.TempStrength,
        //fire
        ActivationVars.Burn,
        //earth
        ActivationVars.Block,
        //metal
        ActivationVars.Vigor,
        
        new BoolVar("isWaterOn"),
        new BoolVar("isWoodOn"),
        new BoolVar("isFireOn"),
        new BoolVar("isEarthOn"),
        new BoolVar("isMetalOn"),
    ]);
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Water),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Wood),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fire),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Earth),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Metal),
        HoverTipFactory.FromPower<WavePower>(),
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        if (CombatState == null) return;
        if (CardElementTag.Water.IsActive(CombatState))
        {
            await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
            await CommonActions.ApplySelf<WavePower>(this, DynamicVars["WavePower"].BaseValue);
        }
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            await CommonActions.Draw(this, choiceContext);
            await CommonActions.ApplySelf<ActivationTempStrengthPower>(this, DynamicVars["ActivationTempStrengthPower"].BaseValue);
        }   
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<BurnPower>(choiceContext,targets, this.DynamicVars["BurnPower"].BaseValue, this.Owner.Creature, this);
        }
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await CommonActions.CardBlock(this, play);
        }
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            await CommonActions.ApplySelf<VigorPower>(this, DynamicVars["VigorPower"].BaseValue);
        }
    }
    protected override void OnUpgrade()
    {
        AddKeyword(FiveElementsKeywords.Attune);
    }
    
    public override TargetType TargetType 
    {
        get
        {
            if (CardElementTag.Fire.IsActive(CombatState))
            {
                return TargetType.AllEnemies;
            }
            return  TargetType.Self;
        }
    } 
    
    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        string? varName = element switch
        {
            CardElementTag.Water => "isWaterOn",
            CardElementTag.Wood => "isWoodOn",
            CardElementTag.Fire => "isFireOn",
            CardElementTag.Earth => "isEarthOn",
            CardElementTag.Metal => "isMetalOn",
            _ => null
        };

        if (varName != null)
        {
            DynamicVars[varName].BaseValue = isActive ? 1 : 0;
        }
        await Task.CompletedTask;
    }

    public CardModel GetTranscendenceTransformedCard()
    {
        return ModelDb.Card<Incarnation>();
    }
}