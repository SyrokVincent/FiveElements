using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._6_Ancient;

public sealed class Incarnation() : NeutralCard(0,
    CardType.Skill, CardRarity.Ancient,
    TargetType.AllEnemies), IOnElementStateChanged
{

    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && Owner.Creature.IsAnyElementActive();

    //Water:(1 energy, 5 wave),
    //Wood:(Draw 1, 3 surge),
    //Fire:(Burn 8 to all enemies),
    //Earth:(10 block),
    //Metal:(5 vigor), 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        
        //water
        new EnergyVar(1),
        new PowerVar<WavePower>(5),
        //wood
        new CardsVar(1),
        new PowerVar<SurgePower>(3),
        //fire
        new PowerVar<BurnPower>(8),
        //earth
        new BlockVar(10,ValueProp.Move),
        //metal
        new PowerVar<VigorPower>(5),
        
        new BoolVar("isWaterOn"),
        new BoolVar("isWoodOn"),
        new BoolVar("isFireOn"),
        new BoolVar("isEarthOn"),
        new BoolVar("isMetalOn"),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

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
            if (CardElementTag.Fire.IsActive(Owner.Creature))
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
}