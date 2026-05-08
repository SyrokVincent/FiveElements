using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace FiveElements.FiveElementsCode.Models;

public class Element
{
    // On lie l'élément à la Créature (le joueur)
    public Creature Owner { get; }
    public Element(Creature owner) 
    {
        Owner = owner;
    }
    
    // Un dictionnaire pour stocker tous les éléments : Clé = Tag, Valeur = Essence
    private readonly Dictionary<CardElementTag, int> _essences = new();
    
    //garde en memoire l'etat des element si ils sont actif ou pas
    private readonly Dictionary<CardElementTag, bool> _lastStatesMemory = new();
    
    // L'ECHO EST MAINTENANT ICI : Unique par instance d'Element (donc par joueur)
    public SortedSet<CardElementTag> Echo { get; } = new() { CardElementTag.Neutral };
    
    // Stockage  : ID de l'entrée d'historique -> Tags au moment du jeu
    public Dictionary<CardPlay, SortedSet<CardElementTag>> PlayedElementsCache { get; } = new();
    public void ClearPlayCache()
    {
        PlayedElementsCache.Clear();
    }
    // --- LE NOUVEL EVENEMENT ---
    // Cet événement transmet l'élément concerné et la nouvelle valeur
    public event Action<CardElementTag, int, PlayerChoiceContext?>? EssenceChanged;

    // Méthodes pour manipuler l'Echo au niveau de l'instance
    public void SetEcho(IEnumerable<CardElementTag> elements)
    {
        Echo.Clear();
        foreach (var e in elements) Echo.Add(e);
    }

    public void ResetEcho()
    {
        Echo.Clear();
        Echo.Add(CardElementTag.Neutral);
    }
    
    public int GetEchoStateForDescription()
    {
        if (Echo.Count == 6) return 6; //echo has all element
        return (int) Echo.LastOrDefault(); //echo has only one element
    }
    
    // Méthode utilitaire pour changer l'écho facilement
    public void SetEchoToAllElements()
    {
        Echo.Clear();
        foreach (var element in Enum.GetValues<CardElementTag>())
        {
            Echo.Add(element);
        }
    }
    
    
    
    
    // Une méthode générique pour modifier n'importe quel élément
    public void AddEssence(CardElementTag elem, int amount, PlayerChoiceContext? context = null)
    {
        int current = GetEssence(elem);
        int next = Math.Max(0, current + amount);

        if (current != next)
        {
            _essences[elem] = next;
            EssenceChanged?.Invoke(elem, next, context);
            
            // On notifie les cartes via le CombatState spécifique de cette créature
            if (Owner.CombatState != null)
            {
                _ = FiveElementsCardExtensions.CheckAndNotify(Owner, elem);
            }
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
        
        // On prévient aussi l'UI que c'est retombé à 0
        EssenceChanged?.Invoke(elem, 0,null);

        // TRES IMPORTANT : On notifie les cartes que l'élément a disparu
        _ = FiveElementsCardExtensions.CheckAndNotify(Owner, elem);
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
    
    public int GetTotalEssenceCount()
    {
        return _essences.Values.Sum();
    }
    
    // Retourne une copie pour éviter que le code extérieur ne modifie le dictionnaire interne
    public Dictionary<CardElementTag, int> GetAllEssences()
    {
        // On ne retourne que les éléments qui ont au moins 1 essence
        return _essences.Where(kvp => kvp.Value > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    

    public bool GetLastState(CardElementTag elem) => 
        _lastStatesMemory.TryGetValue(elem, out bool val) && val;

    public void SetLastState(CardElementTag elem, bool state) => 
        _lastStatesMemory[elem] = state;
}
