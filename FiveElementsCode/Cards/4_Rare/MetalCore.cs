using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class MetalCore() : MetalCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self), IOnEarthStateChanged
{
    
    public override bool GainsBlock => true;

    //delete if shouldn't glow or replace water
    protected override bool ShouldGlowGoldInternal => 
        CombatState != null && 
        (CardElementTag.Earth.IsActive(CombatState) || 
         CardElementTag.Metal.IsActive(CombatState));

    //Earth:(gain 1 vigor for every 3(2) block),(Remove block?), 
    //Metal:(Next turn first attack deal double damage)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isEarthOn"),
        new CalculationBaseVar(0),              //vigor de base
        new CalculationExtraVar(1),   //vigor bonus par block
        new IntVar("BlockDivider",3), // divise la vigor bonus par block
        new CalculatedVar("VigorGained").WithMultiplier((card, target) =>
        {
            // On récupère la valeur actuelle du diviseur (2 ou 3)
            var divider = card.DynamicVars["BlockDivider"].BaseValue;
            
            if (card.CombatState == null || divider <= 0) 
                return 0;
            // On renvoie le multiplicateur (nombre de fois qu'on ajoute CalculationExtraVar)
            var blockAmount = card.Owner.Creature.Block;
            return (decimal)(blockAmount / divider);
        }),
        new PowerVar<MetalCorePower>(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Earth),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Metal),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await PowerCmd.Apply<VigorPower>(choiceContext,this.Owner.Creature, DynamicVars["VigorGained"].PreviewValue, Owner.Creature, this,false);
        }
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            await CommonActions.ApplySelf<MetalCorePower>(choiceContext,this, DynamicVars["MetalCorePower"].BaseValue);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockDivider"].UpgradeValueBy(-1);
    }
    
    
      
    public async Task OnEarthStateChanged(bool isActive)
    {
        DynamicVars["isEarthOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }

    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        if (element == CardElementTag.Earth) await OnEarthStateChanged(isActive);
        if (element == CardElementTag.Metal) await OnMetalStateChanged(isActive);
    }
}