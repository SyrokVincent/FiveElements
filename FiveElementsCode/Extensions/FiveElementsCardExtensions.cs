using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Hooks;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Extensions;

public static class FiveElementsCardExtensions
{
    
    private static async Task SyncElementalState(CardModel potentialListener, CombatState combatState)
    {
        // On vérifie si l'objet écoute les changements d'éléments
        if (potentialListener is IOnElementStateChanged elementalListener)
        {
            // On parcourt toutes les valeurs de l'Enum
            foreach (CardElementTag elem in Enum.GetValues<CardElementTag>())
            {
                // On ignore le tag Neutre/None pour éviter les calculs inutiles
                if (elem == CardElementTag.Neutral) continue;

                // On récupère l'état actuel dans le combat
                bool isActive = elem.IsActive(combatState);

                // On déclenche la mise à jour (via l'interface maître qui redirige vers les filles)
                await elementalListener.OnElementStateChanged(elem, isActive);
            }
        }
    }


    public static async Task TransformInHand<T>(Player owner, IReadOnlyList<CardModel> cards, bool isUpgraded, CombatState combatState) 
        where T : CardModel // On précise que T doit être un modèle de carte
    {
        foreach (var card in cards )
        {
            var replacementCard = combatState.CreateCard<T>(owner);
            
            await SyncElementalState(replacementCard, combatState);

            //if (isUpgraded) CardCmd.Upgrade(replacementCard);
            await CardCmd.Transform(card, replacementCard);
        }
    }
    
    public static async Task CreateInHand<T>(Player owner, int count, bool isUpgraded, CombatState combatState) 
        where T : CardModel // On précise que T doit être un modèle de carte
    {
        var cards = new List<CardModel>();

        for (var i = 0; i < count; i++) 
        {
            var card = combatState.CreateCard<T>(owner);
            
            await SyncElementalState(card, combatState);

            if (isUpgraded) CardCmd.Upgrade(card);
            cards.Add(card);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, true);
    }
    
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

    public static bool IsNeutral(this FiveElementsCard card)
    {
        return card.ElementTags.Contains(CardElementTag.Neutral);
    }
    public static bool IsWater(this FiveElementsCard card)
    {
        return card.ElementTags.Contains(CardElementTag.Water);
    }
    public static bool IsWood(this FiveElementsCard card)
    {
        return card.ElementTags.Contains(CardElementTag.Wood);
    }
    public static bool IsFire(this FiveElementsCard card)
    {
        return card.ElementTags.Contains(CardElementTag.Fire);
    }
    public static bool IsEarth(this FiveElementsCard card)
    {
        return card.ElementTags.Contains(CardElementTag.Earth);
    }
    public static bool IsMetal(this FiveElementsCard card)
    {
        return card.ElementTags.Contains(CardElementTag.Metal);
    }
    
    
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