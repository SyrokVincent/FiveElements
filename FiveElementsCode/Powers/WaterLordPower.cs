using BaseLib.Extensions;
using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class WaterLordPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WavePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        WaterLordVars.WaterLord,
    ]);
    
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        bool shouldTrigger = false;

        // CAS A : Ma propre carte
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            if (cardPlay.Card.CountAsElement(CardElementTag.Water, Owner))
                shouldTrigger = true;
        }
        // CAS B : Carte alliée via le lien (BodyAttunement)
        else if (cardPlay.Card.Owner.HasPower<MindAndBodyAttunementBodyPower>() && Owner.HasPower<MindAndBodyAttunementMindPower>())
        {
            // On vérifie NOTRE Echo
            if (Owner.GetElementalStatus().Echo.Contains(CardElementTag.Water))
                shouldTrigger = true;
        }
        
        // 2. Exécution de l'effet
        if (shouldTrigger)
        {
            // Calcul du montant de wave
            decimal waveAmount = Amount;

            // On ne réduit le montant que si c'est NOUS qui jouons la carte waterlord pour ne pas la compter
            if (cardPlay.Card is WaterLord && cardPlay.Card.Owner.Creature == Owner)
                waveAmount -= DynamicVars["WaterLordPower"].BaseValue;

            if (waveAmount > 0)
            {
                Flash();
                await PowerCmd.Apply<WavePower>(context, Owner, waveAmount, Owner, null);
            }
        }
    }
    
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        await PowerCmd.Remove(this);
    }
}