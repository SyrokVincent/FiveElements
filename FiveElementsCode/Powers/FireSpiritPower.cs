using BaseLib.Extensions;
using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class FireSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        FireSpiritVars.FireSpirit, //this number need to be the same as the one on firespirit
    ]);
    
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 1. Déterminer si la carte doit déclencher l'effet
        bool shouldTrigger = false;

        // CAS A : C'est notre propre carte
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            // On vérifie si elle compte comme Feu (inclut Attune/Shift/Spirits Form)
            if (cardPlay.Card.CountAsElement(CardElementTag.Fire, Owner))
                shouldTrigger = true;
        }
        // CAS B : C'est une carte alliée sous BodyAttunement
        else if (cardPlay.Card.Owner.HasPower<MindAndBodyAttunementBodyPower>() && Owner.HasPower<MindAndBodyAttunementMindPower>())
        {
            // On vérifie si NOTRE Echo actuel est Feu
            if (Owner.GetElementalStatus().Echo.Contains(CardElementTag.Fire))
                shouldTrigger = true;
        }

        // 2. Exécution de l'effet
        if (shouldTrigger)
        {
            // Calcul du montant de Burn
            decimal burnAmount = Amount;

            // On ne réduit le montant que si c'est NOUS qui jouons la carte FireSpirit
            if (cardPlay.Card is FireSpirit && cardPlay.Card.Owner.Creature == Owner)
                burnAmount -= DynamicVars["FireSpiritPower"].BaseValue;

            if (burnAmount > 0)
            {
                Flash();
                var targets = CombatState.HittableEnemies;
                await PowerCmd.Apply<BurnPower>(context, targets, burnAmount, this.Owner, null);
            }
        }
    }
}