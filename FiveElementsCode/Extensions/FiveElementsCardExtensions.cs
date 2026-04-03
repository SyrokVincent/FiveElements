using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards.Token;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Extensions;

public static class FiveElementsCardExtensions
{
    //todo intvar  test copium, pool, hovertips everywhere, fulu
    public static async Task TryShiftFuluTransform(this FiveElementsCard cardToTransform, CardPlay cardPlay)
    {
        var combatState = cardToTransform.CombatState;
        var owner = cardToTransform.Owner;
        if (combatState != null && owner == cardPlay.Card.Owner && cardPlay.Card != cardToTransform && cardToTransform.Pile?.Type != PileType.Exhaust)
        {
            var myElement = cardToTransform.CanonicalElementTags.FirstOrDefault();
            CardModel? replacement = null;
            if (cardPlay.Card is FiveElementsCard elementCard)
            {
                var playedElem = elementCard.CanonicalElementTags.FirstOrDefault();
                replacement = playedElem switch
                {
                    CardElementTag.Water   => combatState.CreateCard<WoodFulu>(owner),
                    CardElementTag.Wood    => combatState.CreateCard<FireFulu>(owner),
                    CardElementTag.Fire    => combatState.CreateCard<EarthFulu>(owner),
                    CardElementTag.Earth   => combatState.CreateCard<MetalFulu>(owner),
                    CardElementTag.Metal   => combatState.CreateCard<WaterFulu>(owner),
                    CardElementTag.Neutral => combatState.CreateCard<Fulu>(owner),
                    _                      => throw new ArgumentOutOfRangeException()
                };
            }
            else 
            {
                replacement = combatState.CreateCard<Fulu>(owner);
            }
            // remove useless transform if card is already of the good element
            if (replacement is FiveElementsCard replacementElementCard)
            {
                var targetElement = replacementElementCard.CanonicalElementTags.FirstOrDefault();
                if (targetElement == myElement) return; //no transformation
            }
            await CardCmd.Transform(cardToTransform, replacement);
        }
    }
    
    
    public static bool IsActive(this CardElementTag elem)
    {
        // On accède aux variables via le nom de ta classe de base (ex: MyBaseCard)
        // Remplace "BaseCard" par le vrai nom de ta classe où se trouvent ces variables
        CardElementTag echo = FiveElementsCard.ElementOfEcho;

        return elem switch
        {
            CardElementTag.Water => (echo == CardElementTag.Water || echo == CardElementTag.Metal || FiveElementsCard.WaterEnergy > 0),
            CardElementTag.Wood  => (echo == CardElementTag.Wood  || echo == CardElementTag.Water || FiveElementsCard.WoodEnergy > 0),
            CardElementTag.Fire  => (echo == CardElementTag.Fire  || echo == CardElementTag.Wood  || FiveElementsCard.FireEnergy > 0),
            CardElementTag.Earth => (echo == CardElementTag.Earth || echo == CardElementTag.Fire  || FiveElementsCard.EarthEnergy > 0),
            CardElementTag.Metal => (echo == CardElementTag.Metal || echo == CardElementTag.Earth || FiveElementsCard.MetalEnergy > 0),
            _ => false
        };
    }

    public static bool IsAnyElementActive()
    {
        return CardElementTag.Water.IsActive() ||
               CardElementTag.Wood.IsActive()  ||
               CardElementTag.Fire.IsActive()  ||
               CardElementTag.Earth.IsActive() ||
               CardElementTag.Metal.IsActive();
    }
}