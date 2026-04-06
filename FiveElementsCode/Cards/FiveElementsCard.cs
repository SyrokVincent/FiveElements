using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards.Token;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Cards;

[Pool(typeof(FiveElementsCardPool))]
public abstract class FiveElementsCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target), IOnElementStateChanged
{
  
    

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("water_s",FiveElementsColor.WaterColor),
        new StringVar("water_e","[/color]"),
        new StringVar("wood_s",FiveElementsColor.WoodColor),
        new StringVar("wood_e","[/color]"),
        new StringVar("fire_s",FiveElementsColor.FireColor),
        new StringVar("fire_e", "[/color]"),
        new StringVar("earth_s", FiveElementsColor.EarthColor),
        new StringVar("earth_e", "[/color]"),
        new StringVar("metal_s", FiveElementsColor.MetalColor),
        new StringVar("metal_e", "[/color]"),
        new StringVar("off_s", FiveElementsColor.OffColor ),
        new StringVar("off_e", "[/color]" ),
    ];
    
    
    /*
    protected FiveElementsCard(int cost, CardType type, CardRarity rarity, TargetType target, CardElementTag elem):
        base(cost, type, rarity, target)
    {
        ElementField.ElementType.Set(this, elem);
        // Debug : earth est bien set, il est 
        var check = ElementField.ElementType.Get(this);
        GD.Print($"Carte créée. Élément demandé: {elem}, Élément stocké: {check}");
        int instanceId = this.GetHashCode();
        GD.Print($"[DEBUG] Création Carte ID: {instanceId} | Element: {ElementField.ElementType.Get(this)}");
    }
    */
    
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    //public override string CustomPortraitPath => "card.png".BigCardImagePath();

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    //public override string PortraitPath => "card.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    
    
    // stuff I added
    private HashSet<CardElementTag> _canonicalElementTags = [CardElementTag.Neutral];

    private HashSet<CardElementTag>? _liveElementTags;

    public virtual IEnumerable<CardElementTag> ElementTags => 
        _liveElementTags ??= new HashSet<CardElementTag>(_canonicalElementTags);

    public virtual HashSet<CardElementTag> CanonicalElementTags
    {
        get => _canonicalElementTags;
        set 
        {
            _canonicalElementTags = value;
            // IMPORTANT : Si on change le canonique, on force la régénération du live
            _liveElementTags = null; 
        }
    }
    
    
    
    
    public static async Task CreateInHand<T>(Player owner, int count, bool isUpgraded, CombatState combatState) 
        where T : CardModel // On précise que T doit être un modèle de carte
    {
        var cards = new List<CardModel>();

        for (var i = 0; i < count; i++) 
        {
            var card = combatState.CreateCard<T>(owner);
        
            // --- FORCER LA MISE À JOUR INITIALE ---
            // On vérifie manuellement chaque élément pour la nouvelle carte
            foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
            {
                bool isActive = elem.IsActive(combatState);
                // On appelle la fonction de mise à jour visuelle/logique de la carte
                // Assure-toi que ta carte a une méthode publique pour ça
                if (card is FiveElementsCard elementalCard) 
                {
                    await elementalCard.OnElementStateChanged(elem, isActive);
                }
            }

            if (isUpgraded) CardCmd.Upgrade(card);
            cards.Add(card);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, true);
    }

    public abstract Task OnElementStateChanged(CardElementTag element, bool isActive);
}  
    
    /*
    public CardElementTag ElementOfLastCardPlayed
    {
        get
        {
            CardPlayStartedEntry playStartedEntry = CombatManager.Instance.History.CardPlaysStarted.LastOrDefault<CardPlayStartedEntry>((Func<CardPlayStartedEntry, bool>) (e =>
                e.CardPlay.Card.Owner == this.Owner &&
                e.HappenedThisTurn(this.CombatState) &&
                e.CardPlay.Card != this));
            //return playStartedEntry != null && playStartedEntry.CardPlay.Card.Type == CardType.Skill;

            if (playStartedEntry != null)
            {
                var t = (FiveElementsCard)playStartedEntry.CardPlay.Card;
                return t.ElementTags.Single();
            }
            return CardElementTag.Neutral;
        }
    }
    
    
    
    public CardElementTag ElementOfEcho
    {
        get
        {
            var combatState = this.CombatState;
            if (combatState != null) return combatState.GetElementalStatus().ElementOfEcho;
            return CardElementTag.Neutral;
        }
    }

    //todo nothing to do here, need to put it somewhere logical
    //useless for now might be usefull later ?
    protected HoverTip StaticHoverTip(string str, IEnumerable<DynamicVar> vars)
    {
        var title = new LocString("static_hover_tips", str + ".title");
        var description = new LocString("static_hover_tips", str + ".description");
        foreach (DynamicVar var in vars)
        {
            title.Add(var);
            description.Add(var);
        }

        if (str == "FIVEELEMENTS-ECHO")
        {
            var elemEcho = new IntVar("ElemEcho", (decimal)ElementOfEcho);
            title.Add(elemEcho);
            description.Add(elemEcho);
        }
        return new HoverTip(title, description);
    }

   */




/*
public class ElementField
{
    public static readonly SpireField<CardModel, CardElementTag> ElementType = new(() => CardElementTag.Neutral); //Default value to neutral

}
*/