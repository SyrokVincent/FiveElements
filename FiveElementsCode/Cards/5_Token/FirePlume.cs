using BaseLib.Extensions;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._5_Token;

[Pool(typeof(TokenCardPool))]
public sealed class FirePlume() : FireCard(0,
    CardType.Attack, CardRarity.Token,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(CombatState);

    //Retain, Exhaust, At turn start while in hand apply 1 burn to all enemies, (keyword Incandescence 1)
    //Fire:(Deal 1 Heat damage, increased by 1 for each fire card played this turn)
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<BurnPower>(1),
        new CalculationBaseVar(1),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => 
        {
            if (card.CombatState == null) return 0;
            
            return CombatManager.Instance.History.CardPlaysFinished.Count(e => 
            {
                if (!e.HappenedThisTurn(card.CombatState) || e.CardPlay.Card.Owner != card.Owner)
                    return false;
                
                return (e.CardPlay.Card.CountsAsElement(CardElementTag.Fire, card.Owner.Creature));
            });
        }),
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
        
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            await DealHeatDamage(choiceContext, play.Target, DynamicVars.CalculatedDamage);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(1);
        DynamicVars["BurnPower"].UpgradeValueBy(1);
    }
    
    
    // when fire rise tirgger with that it go to the right of card draw and don't trigger incandescence
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        //it's Incandescence triggering
        if (CombatState != null && PileType.Hand.GetPile(Owner).Cards.Contains(this))
            foreach (var hittableEnemy in CombatState.HittableEnemies)
            {
                await CommonActions.Apply<BurnPower>(hittableEnemy, this, DynamicVars["BurnPower"].BaseValue);
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