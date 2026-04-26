using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

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
        new IntVar("ElemEcho",0)
    ]);

    
    private List<CardElementTag> _currentEchoSnapshot = new();
    // On stocke les éléments que la carte "avait" au moment du clic
    private HashSet<CardElementTag> _cardElementsBeforePlay = new();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // 1. Snapshot de l'Echo global
        _currentEchoSnapshot = Character.FiveElements.Echo.ToList();

        // 2. Snapshot des éléments de la carte AVANT qu'elle ne change
        _cardElementsBeforePlay.Clear();
        var card = cardPlay.Card;

        // On vérifie tous les éléments possibles via CountAsElement
        // car cela inclut Attune/Shift calculé au moment T
        if (card.CountAsElement(CardElementTag.Water, Owner)) _cardElementsBeforePlay.Add(CardElementTag.Water);
        if (card.CountAsElement(CardElementTag.Wood, Owner))  _cardElementsBeforePlay.Add(CardElementTag.Wood);
        if (card.CountAsElement(CardElementTag.Fire, Owner))  _cardElementsBeforePlay.Add(CardElementTag.Fire);
        if (card.CountAsElement(CardElementTag.Earth, Owner)) _cardElementsBeforePlay.Add(CardElementTag.Earth);
        if (card.CountAsElement(CardElementTag.Metal, Owner)) _cardElementsBeforePlay.Add(CardElementTag.Metal);

        return Task.CompletedTask;
    }
    
    
    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // Sécurité de base
        if (Owner != cardPlay.Card.Owner.Creature || Owner.Player == null) 
            return;

        
        // On utilise nos snapshots capturés dans BeforeCardPlayed
        var snapshotEcho = _currentEchoSnapshot;
        var cardWas = _cardElementsBeforePlay;
        
        /*
        var elementCard = cardPlay.Card as FiveElementsCard;
        if (elementCard == null && !HasSpiritsForm)
        {
            DynamicVars["ElemEcho"].BaseValue = (decimal)CardElementTag.Neutral;
            return;
        }*/

        var elemOfCardPlay = cardWas.LastOrDefault();
        DynamicVars["ElemEcho"].BaseValue = (decimal)elemOfCardPlay;

        string strcardWas = string.Join(", ", cardWas);
        string strsnapshotEcho = string.Join(", ", snapshotEcho);
        GD.Print($"DEBUG: ultimateform elemOfCardPlay=[{elemOfCardPlay}] strcardWas={strcardWas}  strsnapshotEcho=[{strsnapshotEcho}");
        
        var needUpgrade = false;

        // 1. Déclenchement des effets selon l'élément
        if (cardWas.Contains(CardElementTag.Water) && snapshotEcho.IsGenerating(CardElementTag.Water))
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue * Amount, Owner.Player);
            await PowerCmd.Apply<WavePower>(context, Owner, DynamicVars["WavePower"].BaseValue * Amount, Owner, null);
            needUpgrade = true;
        }
        if (cardWas.Contains(CardElementTag.Wood) && snapshotEcho.IsGenerating(CardElementTag.Wood))
        {
            await CardPileCmd.Draw(context, DynamicVars.Cards.BaseValue * Amount, Owner.Player);
            await PowerCmd.Apply<SurgePower>(context, Owner, DynamicVars["SurgePower"].BaseValue * Amount, Owner, null);
            needUpgrade = true;
        }
        if (cardWas.Contains(CardElementTag.Fire) && snapshotEcho.IsGenerating(CardElementTag.Fire))
        {
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<BurnPower>(context, targets, this.DynamicVars["BurnPower"].BaseValue * Amount, this.Owner, null);
            needUpgrade = true;
        }
        if (cardWas.Contains(CardElementTag.Earth) && snapshotEcho.IsGenerating(CardElementTag.Earth))
        {
            await CreatureCmd.GainBlock(Owner, DynamicVars.Block.BaseValue * Amount, DynamicVars.Block.Props, null);
            needUpgrade = true;
        }
        if (cardWas.Contains(CardElementTag.Metal) && snapshotEcho.IsGenerating(CardElementTag.Metal))
        {
            await PowerCmd.Apply<VigorPower>(context, Owner, DynamicVars["VigorPower"].BaseValue * Amount, Owner, null);
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