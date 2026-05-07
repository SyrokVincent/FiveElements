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
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class MetalSpiritPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        MetalSpiritVars.MetalSpirit, //need to be the same number as on metalspiritPower
    ]);
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 1. Déterminer si la carte doit déclencher l'effet
        bool shouldTrigger = false;

        // CAS A : C'est notre propre carte
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            // On vérifie si elle compte comme Métal (inclut Attune/Shift/Spirits Form)
            if (cardPlay.Card.CountAsElement(CardElementTag.Metal, Owner))
                shouldTrigger = true;
        }
        // CAS B : C'est une carte alliée sous BodyAttunement
        else if (cardPlay.Card.Owner.HasPower<MindAndBodyAttunementBodyPower>() && Owner.HasPower<MindAndBodyAttunementMindPower>())
        {
            // On vérifie si NOTRE Echo actuel est Métal
            if (Owner.GetElementalStatus().Echo.Contains(CardElementTag.Metal))
                shouldTrigger = true;
        }

        // 2. Exécution de l'effet
        if (shouldTrigger)
        {
            // Calcul du montant de Vigor
            decimal vigorAmount = Amount;

            // On ne réduit le montant que si c'est NOUS qui jouons la carte MetalSpirit
            if (cardPlay.Card is MetalSpirit && cardPlay.Card.Owner.Creature == Owner)
                vigorAmount -= DynamicVars["MetalSpiritPower"].BaseValue;

            if (vigorAmount > 0)
            {
                Flash();
                await PowerCmd.Apply<VigorPower>(context, Owner, vigorAmount, Owner, null);
            }
        }
    }
}