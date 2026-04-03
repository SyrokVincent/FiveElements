using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;

namespace FiveElements.FiveElementsCode.Models;

public class Element
{
    public CombatState CombatState { get; }
    
    // Un dictionnaire pour stocker tous les éléments : Clé = Tag, Valeur = Essence
    private readonly Dictionary<CardElementTag, int> _essences = new();

    public Element(CombatState combatState) => CombatState = combatState;

    // Une méthode générique pour modifier n'importe quel élément
    public void AddEssence(CardElementTag elem, int amount)
    {
        int current = GetEssence(elem);
        int next = Math.Max(0, current + amount); // On évite les essences négatives

        if (current != next)
        {
            _essences[elem] = next;
            // On notifie les cartes du changement pour cet élément spécifique
            _ = FiveElementsCardExtensions.CheckAndNotify(CombatState, elem);
        }
    } 
    /*
    private static CardElementTag _elementOfEcho = CardElementTag.Neutral;

    public CardElementTag ElementOfEcho
    {
        get => _elementOfEcho;
        set
        {
            if (_elementOfEcho == value) return;
            _elementOfEcho = value;

            // Dès que l'Echo change, on prévient TOUS les éléments 
            // car l'Echo d'un élément peut activer le suivant (ex: Métal active Eau)
            // On fait une petite boucle pour notifier chaque élément potentiellement impacté
            foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
            {
                _ = FiveElementsCardExtensions.CheckAndNotify(CombatState, elem);
            }
        }
    }
    */

    public void ResetEssence(CardElementTag elem)
    {
        // On vérifie si l'essence actuelle n'est pas déjà à 0
        if (GetEssence(elem) == 0) return;

        // On remet à zéro dans le dictionnaire
        _essences[elem] = 0;

        // TRES IMPORTANT : On notifie les cartes que l'élément a disparu
        // Sinon tes WaterBubble resteront à 0 de coût !
        _ = FiveElementsCardExtensions.CheckAndNotify(CombatState, elem);
    }

// Bonus : Une méthode pour TOUT reset d'un coup (fin de combat par ex)
    public void ResetAllEssences()
    {
        // On récupère la liste des clés pour éviter les erreurs de modification pendant la boucle
        var tags = _essences.Keys.ToList();
        foreach (var tag in tags)
        {
            ResetEssence(tag);
        }
    }

    public int GetEssence(CardElementTag elem)
    {
        return _essences.TryGetValue(elem, out int value) ? value : 0;
    }
}