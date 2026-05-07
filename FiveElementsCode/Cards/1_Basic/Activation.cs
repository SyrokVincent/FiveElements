using BaseLib.Abstracts;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._6_Ancient;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
    public static PowerVar<SurgePower> Surge => new PowerVar<SurgePower>(1);
    public static PowerVar<BurnPower> Burn => new PowerVar<BurnPower>(3);
    public static BlockVar Block => new BlockVar(5, ValueProp.Move);
    public static PowerVar<VigorPower> Vigor => new PowerVar<VigorPower>(3);
}


public sealed class Activation() : NeutralCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.AllEnemies), IOnElementStateChanged, ITranscendenceCard
{
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    protected override bool ShouldGlowGoldInternal => CombatState != null && Owner.Creature.IsAnyElementActive();
    
    // Water:(1 energy, 3 wave),
    // Wood:(Draw 1 and 1 temp str),
    // Fire:(Burn 3 to all enemies),
    // Earth:(5 block),
    // Metal:(3 vigor) 
    //
    // added 1 temp str and 1 wave, buffed vigor to 3
    //
    //VALUE HERE need to be the same as on Ultimate form and others similar, that why I use Activationvars
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        
        //water
        ActivationVars.Energy,
        ActivationVars.Wave,
        //wood
        ActivationVars.Cards,
        ActivationVars.Surge,
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
    
  
    protected override IEnumerable<IHoverTip> ExtraHoverTips 
    {
        get
        {
            var tips = new List<IHoverTip>();

            // 1. GESTION DU MODE CANONIQUE (Bibliothèque / Hors Combat)
            if (IsCanonical || Owner?.Creature == null)
            {
                if (IsUpgraded) tips.Add(HoverTipFactory.FromKeyword(FiveElementsKeywords.Attune));

                tips.Add(HoverTipFactory.FromPower<WavePower>());
                tips.Add(HoverTipFactory.FromPower<SurgePower>());
                tips.Add(HoverTipFactory.FromPower<BurnPower>());
                tips.Add(HoverTipFactory.Static(StaticHoverTip.Block));
                tips.Add(HoverTipFactory.FromPower<VigorPower>());

                return tips; 
            }
            
            // 2. LOGIQUE DE COMBAT (Si on arrive ici, on est sûr d'avoir un Owner)
            if (IsUpgraded)
            {
                tips.Add(HoverTipFactory.FromKeyword(FiveElementsKeywords.Attune));
            }
            tips.Add(HoverTipFactory.FromKeyword(FiveElementsKeywords.Element));
            tips.Add(HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate));
            tips.Add(HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo));
            
            if (CardElementTag.Water.IsActive(Owner.Creature)) tips.Add(HoverTipFactory.FromPower<WavePower>());
            if (CardElementTag.Wood.IsActive(Owner.Creature))  tips.Add(HoverTipFactory.FromPower<SurgePower>());
            if (CardElementTag.Fire.IsActive(Owner.Creature))  tips.Add(HoverTipFactory.FromPower<BurnPower>());
            // Pour Earth, vu que GainsBlock est à true, le tooltip "Block" 
            // s'ajoute automatiquement via la classe de base, c'est just to place it before vigor un patch en plus gere si on doit l'enlever ou pas
            if (CardElementTag.Earth.IsActive(Owner.Creature)) tips.Add(HoverTipFactory.Static(StaticHoverTip.Block));
            if (CardElementTag.Metal.IsActive(Owner.Creature)) tips.Add(HoverTipFactory.FromPower<VigorPower>());

            return tips;
        }
    }
    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        if (CombatState == null) return;
        if (CardElementTag.Water.IsActive(Owner.Creature))
        {
            await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
            await CommonActions.ApplySelf<WavePower>(choiceContext,this, DynamicVars["WavePower"].BaseValue);
        }
        if (CardElementTag.Wood.IsActive(Owner.Creature))
        {
            await CommonActions.Draw(this, choiceContext);
            await CommonActions.ApplySelf<SurgePower>(choiceContext,this, DynamicVars["SurgePower"].BaseValue);
        }   
        if (CardElementTag.Fire.IsActive(Owner.Creature))
        {
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<BurnPower>(choiceContext,targets, this.DynamicVars["BurnPower"].BaseValue, this.Owner.Creature, this);
        }
        if (CardElementTag.Earth.IsActive(Owner.Creature))
        {
            await CommonActions.CardBlock(this, play);
        }
        if (CardElementTag.Metal.IsActive(Owner.Creature))
        {
            await CommonActions.ApplySelf<VigorPower>(choiceContext,this, DynamicVars["VigorPower"].BaseValue);
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
            // 1. Protection indispensable pour la bibliothèque
            if (IsCanonical || Owner?.Creature == null)
            {
                return TargetType.Self;
            }
            if (CombatState != null && CardElementTag.Fire.IsActive(Owner.Creature))
            {
                return TargetType.AllEnemies;
            }
            return  TargetType.Self;
        }
    } 
    
    public async Task OnElementStateChanged(CardElementTag element, bool isActive, Creature creature)
    {
        if (Owner.Creature != creature) return;
        
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