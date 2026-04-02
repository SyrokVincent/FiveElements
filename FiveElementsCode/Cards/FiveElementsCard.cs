using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Localization;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Cards;

[Pool(typeof(FiveElementsCardPool))]
public abstract class FiveElementsCard(int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true,
    bool autoAdd = true)
    : CustomCardModel(cost, type, rarity, target, showInCardLibrary, autoAdd)
{
    public static int WaterEnergy = 0;
    public static int WoodEnergy = 0;
    public static int FireEnergy = 0;
    public static int EarthEnergy = 0;
    public static int MetalEnergy = 0;

    //color for element in cards description
    protected const string WaterColor = "[color=#1E90FF]";
    protected const string WoodColor = "[color=#228B22]";
    protected const string FireColor = "[color=#FF4500]";
    protected const string EarthColor = "[color=#8B4513]";
    protected const string MetalColor = "[color=#C0C0C0]";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("water_s",CardElementTag.Water.IsActive() ? WaterColor : "" ),
        new StringVar("water_e",CardElementTag.Water.IsActive() ? "[/color]" : ""),
        new StringVar("wood_s",CardElementTag.Wood.IsActive() ? WoodColor : ""),
        new StringVar("wood_e",CardElementTag.Wood.IsActive() ? "[/color]" : ""),
        new StringVar("fire_s",CardElementTag.Fire.IsActive() ? FireColor : ""),
        new StringVar("fire_e",CardElementTag.Fire.IsActive() ? "[/color]" : ""),
        new StringVar("earth_s",CardElementTag.Earth.IsActive() ? EarthColor : ""),
        new StringVar("earth_e",CardElementTag.Earth.IsActive() ? "[/color]" : ""),
        new StringVar("metal_s",CardElementTag.Metal.IsActive() ? MetalColor : ""),
        new StringVar("metal_e",CardElementTag.Metal.IsActive() ? "[/color]" : ""),
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
    
    private HashSet<CardElementTag>? _elementTags;
    
    public virtual IEnumerable<CardElementTag> ElementTags =>  _elementTags ?? (IEnumerable<CardElementTag>) (_elementTags = CanonicalElementTags);

    public virtual HashSet<CardElementTag> CanonicalElementTags
    {
        get => _elementTags;
        set => _elementTags = value;
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
    
    */
    
    
    public static CardElementTag ElementOfEcho => Relic1.Echo;

    //todo nothing to do here, need to put it somewhere logical
    //useless for now might be usefull later ?
    protected static HoverTip StaticHoverTip(string str, IEnumerable<DynamicVar> vars)
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