using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards.Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Hooks;
using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Extensions;

public static class FiveElementsCardExtensions
{
    
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
    
    /* todo delete if everything work lol
    public static bool IsActive(this CardElementTag elem)
    {
        // On accède aux variables via le nom de ta classe de base (ex: MyBaseCard)
        // Remplace "BaseCard" par le vrai nom de ta classe où se trouvent ces variables
        CardElementTag echo = FiveElementsCard.ElementOfEcho;

        return elem switch
        {
            CardElementTag.Water => (echo == CardElementTag.Water || echo == CardElementTag.Metal || FiveElementsCard.WaterEssence > 0),
            CardElementTag.Wood  => (echo == CardElementTag.Wood  || echo == CardElementTag.Water || FiveElementsCard.WoodEssence > 0),
            CardElementTag.Fire  => (echo == CardElementTag.Fire  || echo == CardElementTag.Wood  || FiveElementsCard.FireEssence > 0),
            CardElementTag.Earth => (echo == CardElementTag.Earth || echo == CardElementTag.Fire  || FiveElementsCard.EarthEssence > 0),
            CardElementTag.Metal => (echo == CardElementTag.Metal || echo == CardElementTag.Earth || FiveElementsCard.MetalEssence > 0),
            _ => false
        };
    }
    
    public static bool IsActive(this CardElementTag elem, CombatState combatState)
    {
        // On récupère ton objet Element via l'extension qu'on a créée plus tôt
        var status = combatState.GetElementalStatus();
    
        // On récupère l'Echo (qui est peut-être resté statique ou qui est dans status)
        CardElementTag echo = FiveElementsCard.ElementOfEcho;

        return elem switch
        {
            CardElementTag.Water => echo == CardElementTag.Water || echo == CardElementTag.Metal || status.GetEssence(CardElementTag.Water) > 0,
            CardElementTag.Wood  => echo == CardElementTag.Wood  || echo == CardElementTag.Water || status.GetEssence(CardElementTag.Wood) > 0,
            CardElementTag.Fire  => echo == CardElementTag.Fire  || echo == CardElementTag.Wood  || status.GetEssence(CardElementTag.Fire) > 0,
            CardElementTag.Earth => echo == CardElementTag.Earth || echo == CardElementTag.Fire  || status.GetEssence(CardElementTag.Earth) > 0,
            CardElementTag.Metal => echo == CardElementTag.Metal || echo == CardElementTag.Earth || status.GetEssence(CardElementTag.Metal) > 0,
            _ => false
        };
    }
    */
    public static bool IsActive(this CardElementTag elem, CombatState? combatState)
    {
        if (combatState == null) return false;

        var status = combatState.GetElementalStatus();
        // ON LIT L'ECHO ICI MAINTENANT :
        CardElementTag currentEcho = Character.FiveElements.Echo; //status.ElementOfEcho;
        return elem switch
        {
            CardElementTag.Water => currentEcho == CardElementTag.Water || currentEcho == CardElementTag.Metal || status.GetEssence(CardElementTag.Water) > 0,
            CardElementTag.Wood  => currentEcho == CardElementTag.Wood  || currentEcho == CardElementTag.Water || status.GetEssence(CardElementTag.Wood) > 0,
            CardElementTag.Fire  => currentEcho == CardElementTag.Fire  || currentEcho == CardElementTag.Wood  || status.GetEssence(CardElementTag.Fire) > 0,
            CardElementTag.Earth => currentEcho == CardElementTag.Earth || currentEcho == CardElementTag.Fire  || status.GetEssence(CardElementTag.Earth) > 0,
            CardElementTag.Metal => currentEcho == CardElementTag.Metal || currentEcho == CardElementTag.Earth || status.GetEssence(CardElementTag.Metal) > 0,
            _ => false
        };
    }
    

    public static bool IsAnyElementActive(CombatState combatState)
    {
        return CardElementTag.Water.IsActive(combatState) ||
               CardElementTag.Wood.IsActive(combatState)  ||
               CardElementTag.Fire.IsActive(combatState)  ||
               CardElementTag.Earth.IsActive(combatState) ||
               CardElementTag.Metal.IsActive(combatState);
    }
    
    
    
    // Un dictionnaire pour mémoriser l'état de chaque élément (Eau, Bois, etc.)
    private static readonly Dictionary<CardElementTag, bool> _lastStates = new();

    public static async Task CheckAndNotify(CombatState combatState, CardElementTag elem)
    {
        //GD.Print("CheckAndNotify TRIGGERED");
        // 1. On calcule l'état actuel (Essence + Echo) pour cet élément précis
        bool currentState = elem.IsActive(combatState); 

        // 2. On récupère l'ancien état (false par défaut si c'est la première fois)
        _lastStates.TryGetValue(elem, out bool lastState);

        // 3. On ne déclenche le trigger QUE si l'état a changé
        if (currentState != lastState)
        {
            _lastStates[elem] = currentState;

            // ON COMPLETE LE TRIGGER ICI :
            // On passe 'elem' (le tag) et 'currentState' (le nouveau booléen)
            await MyModHooks.TriggerElementStateChanged(
                combatState.RunState, 
                combatState, 
                elem, 
                currentState
            );
        }
    }
}