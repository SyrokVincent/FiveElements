using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class NeutralCard : FiveElementsCard
{
    protected NeutralCard(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target)
    {
        CanonicalElementTags = [CardElementTag.Neutral];
    }
    
    public override Material? CreateCustomFrameMaterial => NeutralShader;
    
    //je change directement l'elementag fianelement
    /*
    public override HashSet<CardElementTag> ElementTags 
    {
        get 
        {
            // 1. Vérifie si la carte a le Keyword Attune
            if (Keywords.Contains(FiveElementsKeywords.Attune))
            {
                return Character.FiveElements.Echo;
            }
            // 1. Vérifie si la carte a le Keyword Shift
            if (Keywords.Contains(FiveElementsKeywords.Shift))
            {
                // return what echo generate
            
                HashSet<CardElementTag> newEcho = new(){ CardElementTag.Neutral };
                if (Character.FiveElements.Echo.Contains(CardElementTag.Water)) newEcho.Add(CardElementTag.Wood);
                if (Character.FiveElements.Echo.Contains(CardElementTag.Wood)) newEcho.Add(CardElementTag.Fire);
                if (Character.FiveElements.Echo.Contains(CardElementTag.Fire)) newEcho.Add(CardElementTag.Earth);
                if (Character.FiveElements.Echo.Contains(CardElementTag.Earth)) newEcho.Add(CardElementTag.Metal);
                if (Character.FiveElements.Echo.Contains(CardElementTag.Metal)) newEcho.Add(CardElementTag.Water);
                
                return newEcho;
            }

            // 2. Si pas de Attune ni Shift, on utilise le comportement de base de FiveElementsCard
            return base.ElementTags;
        }
    }*/

    //late pour que ce soit apres que l'echo ai changer comme il faut
    public override Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (Owner != cardPlay.Card.Owner || cardPlay.Card == this) return Task.CompletedTask;
        
        // On vérifie si la pile actuelle est une pile de COMBAT (Main, Pioche, Défausse, Exhaust)
        // Si la carte est dans l'historique ou le deck de base, Pile sera null ou non-combat.
        if (this.Pile == null || !this.Pile.Type.IsCombatPile()) 
        {
            return Task.CompletedTask;
        }
        
        // 1. Vérifie si la carte a le Keyword Attune
        if (Keywords.Contains(FiveElementsKeywords.Attune))
        {
            this.ElementTags = Character.FiveElements.Echo;
        }
        // 1. Vérifie si la carte a le Keyword Shift
        if (Keywords.Contains(FiveElementsKeywords.Shift))
        {
            // return what echo generate
        
            HashSet<CardElementTag> newEcho = new(){ CardElementTag.Neutral };
            if (Character.FiveElements.Echo.Contains(CardElementTag.Water)) newEcho.Add(CardElementTag.Wood);
            if (Character.FiveElements.Echo.Contains(CardElementTag.Wood)) newEcho.Add(CardElementTag.Fire);
            if (Character.FiveElements.Echo.Contains(CardElementTag.Fire)) newEcho.Add(CardElementTag.Earth);
            if (Character.FiveElements.Echo.Contains(CardElementTag.Earth)) newEcho.Add(CardElementTag.Metal);
            if (Character.FiveElements.Echo.Contains(CardElementTag.Metal)) newEcho.Add(CardElementTag.Water);
            
            this.ElementTags = newEcho;
        }

        return Task.CompletedTask;
    }
    
    // Stockage statique : ID de l'entrée d'historique -> Tags au moment du jeu
    public static readonly Dictionary<CardPlay, HashSet<CardElementTag>> PlayedElementsCache = new();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // On capture l'état ACTUEL (avant que l'Echo ne change peut-être à la fin du tour)
        PlayedElementsCache[play] = new HashSet<CardElementTag>(this.ElementTags);
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        // On vide le dictionnaire pour libérer les références CardPlay et HashSet
        // Cela garantit que le combat suivant repart sur une base propre
        PlayedElementsCache.Clear();

        return Task.CompletedTask;
    }
}
