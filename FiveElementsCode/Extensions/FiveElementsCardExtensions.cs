using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Hooks;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Extensions;

public static class FiveElementsCardExtensions
{
    
    public static async Task SyncElementalState(CardModel potentialListener, CombatState combatState)
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

    public static async Task TransformInHand(CardModel card, CardModel intoCard, bool needUpgrade, CombatState combatState) 
    {
            await SyncElementalState(intoCard, combatState);
            if (needUpgrade) CardCmd.Upgrade(intoCard);
            await CardCmd.Transform(card, intoCard);
    }
    
    public static async Task TransformInHand<T>(Player owner, IEnumerable<CardModel> cards, bool needUpgrade, CombatState combatState) 
        where T : CardModel // On précise que T doit être un modèle de carte
    {
        foreach (var card in cards )
        {
            var replacementCard = combatState.CreateCard<T>(owner);
            
            await SyncElementalState(replacementCard, combatState);

            if (needUpgrade) CardCmd.Upgrade(replacementCard);
            await CardCmd.Transform(card, replacementCard);
        }
    }
    
    public static async Task CreateInHand<T>(Player owner, int count, bool needUpgrade, CombatState combatState) 
        where T : CardModel // On précise que T doit être un modèle de carte
    {
        var cards = new List<CardModel>();

        for (var i = 0; i < count; i++) 
        {
            var card = combatState.CreateCard<T>(owner);
            
            await SyncElementalState(card, combatState);

            if (needUpgrade) CardCmd.Upgrade(card);
            
            cards.Add(card);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, true);
    }
    
    public static async Task TryShiftFuluTransform(this FiveElementsCard cardToTransform)
    {   
        var combatState = cardToTransform.CombatState;
        var owner = cardToTransform.Owner;

        // Sécurités de base
        if (combatState == null) return;

        // Empêcher la transformation si la carte n'est plus "jouable" (Exil)
        if (cardToTransform.Pile?.Type == PileType.Exhaust) return;

        var currentEcho = Character.FiveElements.Echo;
        CardModel? replacement = null;

     
        // Si l'Echo a les 5 éléments -> on a tout les element grace au pouvoir spirit form on renvoie un fulu neutre
        if (currentEcho.Count >= 5)
        {
            if (cardToTransform is not Fulu) // Evite de transformer un Fulu en lui-même
                replacement = combatState.CreateCard<Fulu>(owner);
        }
        // Cycle : Eau -> Bois -> Feu -> Terre -> Métal -> Eau
        else if (currentEcho.TagsCountAsElement(CardElementTag.Water, owner.Creature))
            replacement = cardToTransform is WoodFulu ? null : combatState.CreateCard<WoodFulu>(owner);
        
        else if (currentEcho.TagsCountAsElement(CardElementTag.Wood, owner.Creature))
            replacement = cardToTransform is FireFulu ? null : combatState.CreateCard<FireFulu>(owner);
        
        else if (currentEcho.TagsCountAsElement(CardElementTag.Fire, owner.Creature))
            replacement = cardToTransform is EarthFulu ? null : combatState.CreateCard<EarthFulu>(owner);
        
        else if (currentEcho.TagsCountAsElement(CardElementTag.Earth, owner.Creature))
            replacement = cardToTransform is MetalFulu ? null : combatState.CreateCard<MetalFulu>(owner);
        
        else if (currentEcho.TagsCountAsElement(CardElementTag.Metal, owner.Creature))
            replacement = cardToTransform is WaterFulu ? null : combatState.CreateCard<WaterFulu>(owner);
        else if (cardToTransform is not Fulu) // Evite de transformer un Fulu en lui-même
            replacement = combatState.CreateCard<Fulu>(owner);
        
        // --- EXÉCUTION DE LA TRANSFORMATION ---
        if (replacement != null && cardToTransform != replacement )
        {
            // 1. Transférer l'upgrade
            if (cardToTransform.IsUpgraded) 
            {
                CardCmd.Upgrade(replacement);
            }
            
            await CardCmd.Transform(cardToTransform, replacement);
        }
    }
    
 

    public static bool IsElement(this FiveElementsCard card, HashSet<CardElementTag> tags)
    {
        return tags.Any(tag => tag != CardElementTag.Neutral && card.ElementTags.Contains(tag));
    }
    public static bool IsElement(this FiveElementsCard card, CardElementTag tag)
    {
        return card.ElementTags.Contains(tag);
    }
    public static bool IsNeutral(this FiveElementsCard card)
    {
        return card.IsElement(CardElementTag.Neutral);
    }
    public static bool IsWater(this FiveElementsCard card)
    {
        return card.IsElement(CardElementTag.Water);
    }
    public static bool IsWood(this FiveElementsCard card)
    {
        return card.IsElement(CardElementTag.Wood);
    }
    public static bool IsFire(this FiveElementsCard card)
    {
        return card.IsElement(CardElementTag.Fire);
    }
    public static bool IsEarth(this FiveElementsCard card)
    {
        return card.IsElement(CardElementTag.Earth);
    }
    public static bool IsMetal(this FiveElementsCard card)
    {
        return card.IsElement(CardElementTag.Metal);
    }
    
    
    public static bool CountAsElement(this CardModel card, HashSet<CardElementTag> tags, Creature owner)
    {
        // 1. Si la carte est déjà de cet élément, c'est bon.
        if (card is FiveElementsCard feCard && feCard.IsElement(tags)) 
            return true;

        // 2. Si le pouvoir SpiritsForm est absent, on s'arrête là.
        if (!owner.HasPower<SpiritsFormPower>())
            return false;

        // 3. Si le pouvoir est présent, il convertit uniquement ce qui n'a pas d'élément.
        if (card is FiveElementsCard feCardNeutral && feCardNeutral.IsNeutral())
            return true;

        if (card is not FiveElementsCard)
            return true;

        return false;
    }
    
    public static bool CountAsElement(this CardModel card, CardElementTag tag, Creature owner)
    {
        // 1. Si la carte est déjà de cet élément, c'est bon.
        if (card is FiveElementsCard feCard && feCard.IsElement(tag)) 
            return true;

        // 2. Si le pouvoir SpiritsForm est absent, on s'arrête là.
        if (!owner.HasPower<SpiritsFormPower>())
            return false;

        // 3. Si le pouvoir est présent, il convertit uniquement ce qui n'a pas d'élément.
        if (card is FiveElementsCard feCardNeutral && feCardNeutral.IsNeutral())
            return true;

        if (card is not FiveElementsCard)
            return true;

        return false;
    }
    
    public static bool TagsCountAsElement(this IEnumerable<CardElementTag> tags, CardElementTag targetTag, Creature owner)
    {
        // On transforme en HashSet pour la performance si c'est une grosse liste
        var tagSet = tags as HashSet<CardElementTag> ?? tags.ToHashSet();

        // 1. Si les tags contiennent déjà l'élément cible
        if (tagSet.Contains(targetTag)) 
            return true;

        // 2. Si le pouvoir SpiritsForm est absent, on s'arrête là
        if (!owner.HasPower<SpiritsFormPower>())
            return false;

        // 3. Si SpiritsForm est présent : 
        // Il convertit les tags s'ils ne contiennent que "Neutral" ou sont vides
        bool isNeutral = tagSet.Count == 0 || (tagSet.Count == 1 && tagSet.Contains(CardElementTag.Neutral));
    
        return isNeutral;
    }
    
    
    
    public static bool IsGenerating(this IEnumerable<CardElementTag> currentEcho, CardElementTag targetElement)
    {
        // Si l'écho contient l'élément qui génère la cible
        // (ex: si Echo contient Wood, il génère Fire)
        return currentEcho.Any(e => e.IsGenerating(targetElement));
    }
    
    public static bool IsGenerating(this CardElementTag elem1, CardElementTag elem2)
    {
        return elem1 switch
        {
            CardElementTag.Water => elem2 == CardElementTag.Wood,
            CardElementTag.Wood  => elem2 == CardElementTag.Fire,
            CardElementTag.Fire  => elem2 == CardElementTag.Earth,
            CardElementTag.Earth => elem2 == CardElementTag.Metal,
            CardElementTag.Metal => elem2 == CardElementTag.Water,
            _ => false
        };
    }
    
    public static bool IsActive(this CardElementTag elem, CombatState? combatState)
    {
        if (combatState == null) return false;

        var status = combatState.GetElementalStatus();
        // ON LIT L'ECHO ICI MAINTENANT :
        HashSet<CardElementTag> currentEcho = Character.FiveElements.Echo; //status.ElementOfEcho;
        return elem switch
        {
            CardElementTag.Water => currentEcho.Contains(CardElementTag.Water) || currentEcho.Contains(CardElementTag.Metal) || status.GetEssence(CardElementTag.Water) > 0,
            CardElementTag.Wood  => currentEcho.Contains(CardElementTag.Wood)  || currentEcho.Contains(CardElementTag.Water) || status.GetEssence(CardElementTag.Wood) > 0,
            CardElementTag.Fire  => currentEcho.Contains(CardElementTag.Fire)  || currentEcho.Contains(CardElementTag.Wood)  || status.GetEssence(CardElementTag.Fire) > 0,
            CardElementTag.Earth => currentEcho.Contains(CardElementTag.Earth) || currentEcho.Contains(CardElementTag.Fire)  || status.GetEssence(CardElementTag.Earth) > 0,
            CardElementTag.Metal => currentEcho.Contains(CardElementTag.Metal) || currentEcho.Contains(CardElementTag.Earth) || status.GetEssence(CardElementTag.Metal) > 0,
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