using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Token;

[Pool(typeof(TokenCardPool))]
public class FirePlume() : FireCard(0,
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
            CombatManager.Instance.History.CardPlaysFinished.Count(e => 
                e.HappenedThisTurn(card.CombatState) && 
                e.CardPlay.Card is FiveElementsCard feCard && feCard.IsFire() && 
                e.CardPlay.Card.Owner == card.Owner)),
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
            await DealHeatDamage(choiceContext, play, DynamicVars.CalculatedDamage);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(1);
    }
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (CombatState != null && PileType.Hand.GetPile(Owner).Cards.Contains(this))
            foreach (var hittableEnemy in CombatState.HittableEnemies)
            {
                await CommonActions.Apply<BurnPower>(hittableEnemy, this, DynamicVars["BurnPower"].BaseValue);
            }
    }
}