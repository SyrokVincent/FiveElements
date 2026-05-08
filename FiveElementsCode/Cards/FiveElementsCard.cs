using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;

[Pool(typeof(FiveElementsCardPool))]
public abstract class FiveElementsCard(int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true, bool autoAdd = true)
    : CustomCardModel(cost, type, rarity, target,showInCardLibrary,autoAdd)
{
    protected static readonly ShaderMaterial WaterShader = ShaderUtils.GenerateHsv(0.6f, 0.9f, 1.0f);
    protected static readonly ShaderMaterial WoodShader = ShaderUtils.GenerateHsv(0.33f, 1.0f, 0.9f);
    protected static readonly ShaderMaterial FireShader = ShaderUtils.GenerateHsv(1.04f, 1.2f, 1.1f);
    protected static readonly ShaderMaterial EarthShader = ShaderUtils.GenerateHsv(0.12f, 0.8f, 0.7f);
    protected static readonly ShaderMaterial MetalShader = ShaderUtils.GenerateHsv(0.55f, 0.05f, 1.2f);
    protected static readonly ShaderMaterial NeutralShader = ShaderUtils.GenerateHsv(1f, 0f, 0.6f);
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("water_s",FiveElementsColor.WaterDescriptionColor),
        new StringVar("water_e","[/color]"),
        new StringVar("wood_s",FiveElementsColor.WoodDescriptionColor),
        new StringVar("wood_e","[/color]"),
        new StringVar("fire_s",FiveElementsColor.FireDescriptionColor),
        new StringVar("fire_e", "[/color]"),
        new StringVar("earth_s", FiveElementsColor.EarthDescriptionColor),
        new StringVar("earth_e", "[/color]"),
        new StringVar("metal_s", FiveElementsColor.MetalDescriptionColor),
        new StringVar("metal_e", "[/color]"),
        new StringVar("off_s", FiveElementsColor.OffDescriptionColor ),
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
    //public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    //public override string CustomPortraitPath => "card.png".BigCardImagePath();
    public override string CustomPortraitPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
            return ResourceLoader.Exists(path) ? path : "card.png".BigCardImagePath();
        }
    }
    
    
    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    //public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    //public override string PortraitPath => "card.png".CardImagePath();
    public override string PortraitPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
            return ResourceLoader.Exists(path) ? path : "card.png".CardImagePath();
        }
    }
    
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    
    
    
    
    // stuff I added
    private SortedSet<CardElementTag> _canonicalElementTags = [CardElementTag.Neutral];

    private SortedSet<CardElementTag>? _liveElementTags;

    public virtual SortedSet<CardElementTag> ElementTags
    {
        get => _liveElementTags ??= new SortedSet<CardElementTag>(_canonicalElementTags);
        set 
        {
            // On met à jour uniquement la version "en jeu"
            _liveElementTags = value;
        
            // On prévient l'UI que la carte a changé visuellement
            //todo trouver un moyen pour mettre a jour le shaderframe
            //this.InvokeKeywordsChanged();
        }
    }
    
    public virtual SortedSet<CardElementTag> CanonicalElementTags
    {
        get => _canonicalElementTags;
        set 
        {
            _canonicalElementTags = value;
            // IMPORTANT : Si on change le canonique, on force la régénération du live
            _liveElementTags = null; 
        }
    }
    
    //maybe history need that to not change element of card with attune and shift already played
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        // On crée une nouvelle instance de SortedSet pour le clone
        this._liveElementTags = new SortedSet<CardElementTag>(this.ElementTags);
    }
    
    //should make description of downgraded card still work
    protected override async void AfterDowngraded()
    {
        base.AfterDowngraded();
        await FiveElementsCardExtensions.SyncElementalState(this, Owner.Creature);
    }
    
    
    /*
    public override Material? CreateCustomFrameMaterial
    {
        get
        {
            // On récupère le dernier élément de la carte (gère Attune automatiquement) //todo might break with spiritform
            var currentElem = ElementTags.LastOrDefault();
            return currentElem switch
            {
                CardElementTag.Water => WaterShader,
                CardElementTag.Wood  => WoodShader,
                CardElementTag.Fire  => FireShader,
                CardElementTag.Earth => EarthShader,
                CardElementTag.Metal => MetalShader, // Blanc/Gris (Sat 0)
                _                    => base.CreateCustomFrameMaterial             // Défaut defini dans le cardpool
            };
        }
    }
    */
}  
    
public static class ElementHistoryUtils
{
    public static int CountPlayedCardsOfElement(ICombatState? combatState, Player? owner, CardElementTag element)
    {
        if (combatState == null || owner == null) return 0;

        // Récupération du pouvoir de lien une seule fois pour la performance
        var power = owner.Creature.GetPower<MindAndBodyAttunementMindPower>();
        var capturedData = power?.GetData()?.CapturedElements;

        return CombatManager.Instance.History.CardPlaysFinished.Count(e => 
        {
            if (!e.HappenedThisTurn(combatState)) return false;

            var playedCard = e.CardPlay.Card;
            // 2. CAS : Carte jouée par MOI
            if (playedCard.Owner == owner)
            {
                var elementStatus = owner.Creature.GetElementalStatus();
                // Si elle est dans le cache (Attune/Shift), on utilise les tags figés
                if (elementStatus.PlayedElementsCache.TryGetValue(e.CardPlay, out var frozenTags))
                {
                    return frozenTags.TagsCountAsElement(element, owner.Creature);
                }
                // Sinon (carte Terre standard), on utilise la méthode normale
                return playedCard.CountAsElement(element, owner.Creature);
            }

            // CAS 2 : Carte d'un allié via le lien
            // On regarde si notre pouvoir a capturé des éléments pour cette carte
            if (capturedData != null && capturedData.TryGetValue(e.CardPlay, out var elements))
            {
                // Si l'Echo qu'on avait au moment où l'allié a joué contient Terre
                return elements.Contains(element);
            }

            return false;
        });
    }
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
            if (combatState != null) return Owner.Creature.GetElementalStatus().ElementOfEcho;
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