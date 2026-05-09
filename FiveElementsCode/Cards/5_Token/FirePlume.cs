using BaseLib.Cards.Variables;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using FiveElements.FiveElementsCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._5_Token;


[Pool(typeof(TokenCardPool))]
public sealed class FirePlume() : FireCard(0,
    CardType.Attack, CardRarity.Token,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(Owner.Creature);

    //Retain, Exhaust, At turn start while in hand apply 1 burn to all enemies, (keyword Incandescence 1)
    //Fire:(Deal 1 Heat damage, increased by 1 for each fire card played this turn)
    // now deal 2 heat damage
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DynamicVar("IncandescenceBase",1),
        new DynamicVar("IncandescenceExtra",1),
        new CustomCalculatedVar("Incandescence").WithMultiplier((CardModel card, Creature? _) => 
             card.Owner?.Relics != null && card.Owner.Relics.Any(r => r is FireRelic)?1:0),

        //new PowerVar<BurnPower>((Owner?.Relics != null && Owner.Relics.Any(r => r is FireRelic))?2:1),
        
        new CalculationBaseVar(2),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => 
            ElementHistoryUtils.CountPlayedCardsOfElement(card.CombatState, card.Owner, CardElementTag.Fire))
    ]);
    
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Retain,
        CardKeyword.Exhaust,
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Incandescence),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Heat),
        HoverTipFactory.FromPower<BurnPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        if (CardElementTag.Fire.IsActive(Owner.Creature))
        {
            await DealHeatDamage(choiceContext, play.Target, DynamicVars.CalculatedDamage);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["IncandescenceBase"].UpgradeValueBy(1);
        DynamicVars.CalculationBase.UpgradeValueBy(1);
    }
    
    
    // when fire rise trigger with that it go to the right of card draw and don't trigger incandescence
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        //it's Incandescence triggering
        if (CombatState != null && PileType.Hand.GetPile(Owner).Cards.Contains(this))
            foreach (var hittableEnemy in CombatState.HittableEnemies)
            {
                await CommonActions.Apply<BurnPower>(choiceContext, hittableEnemy, this, DynamicVars["Incandescence"].PreviewValue);
            }
    }
    
    //test of when is the best trigger
    /*
     // when fire rise tirgger with that it go to the left of card draw and don't trigger incandescence
    public override async Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        //it's Incandescence triggering
        if (CombatState != null && PileType.Hand.GetPile(Owner).Cards.Contains(this))
            foreach (var hittableEnemy in CombatState.HittableEnemies)
            {
                await CommonActions.Apply<BurnPower>(hittableEnemy, this, DynamicVars["BurnPower"].BaseValue);
            }
    }*/
}