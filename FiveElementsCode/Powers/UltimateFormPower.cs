using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

public class UltimateFormPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
        HoverTipFactory.FromCard<Activation>(),
    ];
    
    //VALUE here need to be the same as on activation
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1), 
        new PowerVar<WavePower>(2),
        new CardsVar(1), 
        new PowerVar<BurnPower>(3),
        new BlockVar(4, ValueProp.Move), 
        new PowerVar<VigorPower>(2),
        new IntVar("ElemEcho",0)
    ]);

    private List<CardElementTag> _currentEchoSnapshot = new();
    
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        _currentEchoSnapshot = Character.FiveElements.Echo.ToList();
        return Task.CompletedTask;
    }


    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // Sécurité de base
        if (Owner != cardPlay.Card.Owner.Creature || Owner.Player == null) 
            return;

        var elementCard = cardPlay.Card as FiveElementsCard;
        if (elementCard == null && !HasSpiritsForm)
        {
            DynamicVars["ElemEcho"].BaseValue = (decimal)CardElementTag.Neutral;
            return;
        }
        
        var elemOfCardPlay = elementCard != null ? elementCard.ElementTags.FirstOrDefault() : CardElementTag.Neutral;
        DynamicVars["ElemEcho"].BaseValue = (decimal)elemOfCardPlay;
        
        var needUpgrade = false;

        // 1. Déclenchement des effets selon l'élément
        if (cardPlay.Card.CountsAsElement(CardElementTag.Water,Owner) && _currentEchoSnapshot.IsGenerating(CardElementTag.Water))
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue * Amount, Owner.Player);
            await PowerCmd.Apply<WavePower>(Owner, DynamicVars["WavePower"].BaseValue * Amount, Owner, null);
            needUpgrade = true;
        }
        if (cardPlay.Card.CountsAsElement(CardElementTag.Wood,Owner) && _currentEchoSnapshot.IsGenerating(CardElementTag.Wood))
        {
            await CardPileCmd.Draw(context, DynamicVars.Cards.BaseValue * Amount, Owner.Player);
            needUpgrade = true;
        }
        if (cardPlay.Card.CountsAsElement(CardElementTag.Fire,Owner) && _currentEchoSnapshot.IsGenerating(CardElementTag.Fire))
        {
            foreach (var hittableEnemy in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<BurnPower>(hittableEnemy, DynamicVars["BurnPower"].BaseValue * Amount, Owner, null);
            }
            needUpgrade = true;
        }
        if (cardPlay.Card.CountsAsElement(CardElementTag.Earth,Owner) && _currentEchoSnapshot.IsGenerating(CardElementTag.Earth))
        {
            await CreatureCmd.GainBlock(Owner, DynamicVars.Block.BaseValue * Amount, DynamicVars.Block.Props, null);
            needUpgrade = true;
        }
        if (cardPlay.Card.CountsAsElement(CardElementTag.Metal,Owner) && _currentEchoSnapshot.IsGenerating(CardElementTag.Metal))
        {
            await PowerCmd.Apply<VigorPower>(Owner, DynamicVars["VigorPower"].BaseValue * Amount, Owner, null);
            needUpgrade = true;
        }

        if (needUpgrade)
        {
            this.Flash();
            if (cardPlay.Card.IsUpgradable)
            {
                CardCmd.Upgrade(cardPlay.Card);
            }
        }
    }
    
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        DynamicVars["ElemEcho"].BaseValue = (decimal)CardElementTag.Neutral;
        
    }

}