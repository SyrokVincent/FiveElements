using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public class MultiActivation() : NeutralCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllAllies)
{

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    
    public override bool GainsBlock => false;
    
    //Exhaust?, trigger All the effect on Activation on All players
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
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Attune,
        CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Attune),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromCard<Activation>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        await base.OnPlay(choiceContext, play);
        if (CombatState == null) return;
        
        // On récupère tous les alliés vivants qui sont des joueurs (exclut les invocations/minions)
        var teammates = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(c => c is { IsAlive: true, IsPlayer: true });
        foreach (var teammate in teammates)
        {
            if (teammate.Player != null) await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, teammate.Player);
            await PowerCmd.Apply<WavePower>(choiceContext,teammate, this.DynamicVars["WavePower"].BaseValue, Owner.Creature, this);
  
            if (teammate.Player != null)
                await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, teammate.Player);
            await PowerCmd.Apply<SurgePower>(choiceContext,teammate, this.DynamicVars["SurgePower"].BaseValue, Owner.Creature, this);
   
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<BurnPower>(choiceContext,targets, this.DynamicVars["BurnPower"].BaseValue, Owner.Creature, this);
            
            await CreatureCmd.GainBlock(teammate, DynamicVars.Block, play);
            
            await PowerCmd.Apply<VigorPower>(choiceContext,teammate, this.DynamicVars["VigorPower"].BaseValue, Owner.Creature, this);

        }
    }

    protected override void OnUpgrade()
    {
        this.RemoveKeyword(CardKeyword.Exhaust);
    }
}