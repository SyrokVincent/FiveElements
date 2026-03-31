using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Cards;

[Pool(typeof(FiveElementsCardPool))]
public abstract class FiveElementsCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target)
{
    protected static int WaterEnergy = 0;
    protected static int WoodEnergy = 0;
    protected static int FireEnergy = 0;
    protected static int EarthEnergy = 0;
    protected static int MetalEnergy = 0;

//todo declare 5 color here and use them in cards
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // todo changer les couleur pour les mettres devant et derriere Water,Wood etc
        new StringVar("water_s",IsElementActive(CardElementTag.Water) ? "" : "[color=#1E90FF]"),
        new StringVar("water_e",IsElementActive(CardElementTag.Water) ? "" : "[/color]"),
        new StringVar("wood_s",IsElementActive(CardElementTag.Wood) ? "" : "[color=#228B22]"),
        new StringVar("wood_e",IsElementActive(CardElementTag.Wood) ? "" : "[/color]"),
        new StringVar("fire_s",IsElementActive(CardElementTag.Fire) ? "" : "[color=#FF4500]"),
        new StringVar("fire_e",IsElementActive(CardElementTag.Fire) ? "" : "[/color]"),
        new StringVar("earth_s",IsElementActive(CardElementTag.Earth) ? "" : "[color=#8B4513]"),
        new StringVar("earth_e",IsElementActive(CardElementTag.Earth) ? "" : "[/color]"),
        new StringVar("metal_s",IsElementActive(CardElementTag.Metal) ? "" : "[color=#C0C0C0]"),
        new StringVar("metal_e",IsElementActive(CardElementTag.Metal) ? "" : "[/color]"),
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
    //public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string CustomPortraitPath => "card.png".BigCardImagePath();

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    //public override string PortraitPath => "card.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    
    
    
    // stuff I added
    
    private HashSet<CardElementTag>? _elementTags;
    
    public virtual IEnumerable<CardElementTag> ElementTags =>  _elementTags ?? (IEnumerable<CardElementTag>) (_elementTags = CanonicalElementTags);

    protected virtual HashSet<CardElementTag> CanonicalElementTags => new HashSet<CardElementTag>();
    
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
    
    */

    public static CardElementTag ElementOfEcho => Relic1.Echo;

    public bool IsElementActive(CardElementTag elem)
    {
        switch (elem)
        {
            case CardElementTag.Water: return IsWaterActive();
            case CardElementTag.Wood: return IsWoodActive();
            case CardElementTag.Fire: return IsFireActive();
            case CardElementTag.Earth: return IsEarthActive();
            case CardElementTag.Metal: return IsMetalActive();
            default: return false;
        }
    }
    public bool IsAnyElementActive()
    {
        return IsElementActive(CardElementTag.Water) ||
               IsElementActive(CardElementTag.Wood) ||
               IsElementActive(CardElementTag.Fire) ||
               IsElementActive(CardElementTag.Earth) ||
               IsElementActive(CardElementTag.Metal);
    }

    private static bool IsWaterActive()
    {
        CardElementTag elementOfLastCardPlayed = ElementOfEcho;
        return ((elementOfLastCardPlayed == CardElementTag.Water) ||
                (elementOfLastCardPlayed == CardElementTag.Metal) ||
                WaterEnergy > 0);
    }

    private static bool IsWoodActive()
    {
        CardElementTag elementOfLastCardPlayed = ElementOfEcho;
        return ((elementOfLastCardPlayed == CardElementTag.Wood) ||
                (elementOfLastCardPlayed == CardElementTag.Water) ||
                WoodEnergy > 0);
    }
    private static bool IsFireActive()
    {
        CardElementTag elementOfLastCardPlayed = ElementOfEcho;
        return ((elementOfLastCardPlayed == CardElementTag.Fire) ||
                (elementOfLastCardPlayed == CardElementTag.Wood) ||
                FireEnergy > 0);
    }
    private static bool IsEarthActive()
    {
        CardElementTag elementOfLastCardPlayed = ElementOfEcho;
        return ((elementOfLastCardPlayed == CardElementTag.Earth) ||
                (elementOfLastCardPlayed == CardElementTag.Fire) ||
                EarthEnergy > 0);
    }
    private static bool IsMetalActive()
    {
        CardElementTag elementOfLastCardPlayed = ElementOfEcho;
        return ((elementOfLastCardPlayed == CardElementTag.Metal) ||
                (elementOfLastCardPlayed == CardElementTag.Earth) ||
                MetalEnergy > 0);
    } 
    
/*
    public bool WasLastCardPlayedThisElement(CardElementTag element)
    {
        return ElementOfLastCardPlayed == element;
    }
*/
}
   
public enum CardElementTag
{
    Neutral,
    Water,
    Wood,
    Fire,
    Earth,
    Metal
}
/*
public class ElementField
{
    public static readonly SpireField<CardModel, CardElementTag> ElementType = new(() => CardElementTag.Neutral); //Default value to neutral

}
*/