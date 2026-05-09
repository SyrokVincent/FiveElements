using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rooms;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class NeutralCard : FiveElementsCard
{
    protected NeutralCard(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target)
    {
        CanonicalElementTags = [CardElementTag.Neutral];
    }
    
    
    public override SortedSet<CardElementTag> ElementTags 
    {
        get 
        {
            // On vérifie d'abord si CombatManager existe
            if (CombatManager.Instance?.IsInProgress == true)
            {
                // IMPORTANT : On vérifie IsCanonical pour éviter d'accéder à Owner
                // Si la carte est canonique (bibliothèque), on saute la logique de combat
                if (!IsCanonical && Owner != null)
                {
                    if (Keywords.Contains(FiveElementsKeywords.Attune))
                    {
                        return CalculateAttune(Owner.Creature.GetElementalStatus().Echo);
                    }

                    if (Keywords.Contains(FiveElementsKeywords.Shift))
                    {
                        return CalculateShift(Owner.Creature.GetElementalStatus().Echo);
                    }
                }
            }
        
            // Si hors combat, ou carte canonique, on utilise les tags de base
            return base.ElementTags;
        }
    }
    
    private SortedSet<CardElementTag> CalculateAttune(SortedSet<CardElementTag> echo) 
        => [CardElementTag.Neutral, ..echo];
    
    private SortedSet<CardElementTag> CalculateShift(SortedSet<CardElementTag> echo)
    {
        SortedSet<CardElementTag> newEcho = [CardElementTag.Neutral];
        if (echo.Contains(CardElementTag.Water)) newEcho.Add(CardElementTag.Wood);
        if (echo.Contains(CardElementTag.Wood))  newEcho.Add(CardElementTag.Fire);
        if (echo.Contains(CardElementTag.Fire))  newEcho.Add(CardElementTag.Earth);
        if (echo.Contains(CardElementTag.Earth)) newEcho.Add(CardElementTag.Metal);
        if (echo.Contains(CardElementTag.Metal)) newEcho.Add(CardElementTag.Water);
        return newEcho;
    }
    
    
    public override Material? CreateCustomFrameMaterial
    {
        get
        {
            if (ElementTags.Count >= 5) return NeutralShader;
        
            if (ElementTags.Contains(CardElementTag.Water)) return WaterShader;
            if (ElementTags.Contains(CardElementTag.Wood))  return WoodShader;
            if (ElementTags.Contains(CardElementTag.Fire))  return FireShader;
            if (ElementTags.Contains(CardElementTag.Earth)) return EarthShader;
            if (ElementTags.Contains(CardElementTag.Metal)) return MetalShader;

            return NeutralShader;
        }
    }
    

    //late pour que ce soit apres que l'echo ai changer comme il faut
    public override Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (Owner != cardPlay.Card.Owner || cardPlay.Card == this) return Task.CompletedTask;
        
        UpdateAttuneAndShift();
        //PileType.Draw.GetPile(Owner).InvokeContentsChanged();
        //PileType.Discard.GetPile(Owner).InvokeContentsChanged();

        return Task.CompletedTask;
    }

    //just after echo reset we update for retained card
    public override Task AfterTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player)
        {
            UpdateAttuneAndShift();
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this && card.Owner == Owner)
        {
            UpdateAttuneAndShift();
        }
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
      
        if (player != Owner) return Task.CompletedTask;
        UpdateAttuneAndShift();
        //PileType.Draw.GetPile(Owner).InvokeContentsChanged();
        //PileType.Discard.GetPile(Owner).InvokeContentsChanged();
        return Task.CompletedTask;
    }

    private void UpdateAttuneAndShift()
    {
        //maybe useless?
        // On vérifie si la pile actuelle est une pile de COMBAT (Main, Pioche, Défausse, Exhaust)
        // Si la carte est dans l'historique ou le deck de base, Pile sera null ou non-combat.
        if (this.Pile == null || !this.Pile.Type.IsCombatPile())
        {
            return;
        }
        
        // 1. Vérifie si la carte a le Keyword Attune
        if (Keywords.Contains(FiveElementsKeywords.Attune))
        {
            //this.ElementTags = Owner.Creature.GetElementalStatus().Echo;
            UpdateVisualMaterial();
        }
        // 1. Vérifie si la carte a le Keyword Shift
        if (Keywords.Contains(FiveElementsKeywords.Shift))
        {
            // return what echo generate
            //this.ElementTags = CalculateShift(Owner.Creature.GetElementalStatus().Echo);
            UpdateVisualMaterial();
        }
    }

    public void UpdateVisualMaterial()
    {
        var cardNode = NCard.FindOnTable(this);
        if (cardNode == null) return;

        var frame = cardNode.GetNodeOrNull<CanvasItem>("CardContainer/Frame");
        if (frame != null)
        {
            // 1. On récupère le nouveau matériau basé sur l'élément actuel

            if (CreateCustomFrameMaterial is ShaderMaterial newMat)
            {
                frame.Material = newMat;
                frame.QueueRedraw(); 
            }
        }
    }
    
    
    

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // On récupère l'instance Element du propriétaire de la carte
        var elementStatus = Owner.Creature.GetElementalStatus();
    
        // On stocke les tags actuels dans son cache personnel
        elementStatus.PlayedElementsCache[play] = new SortedSet<CardElementTag>(this.ElementTags);
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        // On vide le dictionnaire pour libérer les références CardPlay et HashSet
        // Cela garantit que le combat suivant repart sur une base propre
        var elementStatus = Owner.Creature.GetElementalStatus();
        elementStatus.ClearPlayCache();

        return Task.CompletedTask;
    }
    
    public override void AfterCreated()
    {
        base.AfterCreated();
        UpdateAttuneAndShift(); 
    }
}
