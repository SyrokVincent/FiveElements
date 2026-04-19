using BaseLib.Abstracts;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Cards._2_Common;
using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Cards._4_Rare;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace FiveElements.FiveElementsCode.Character;

  
  
public class FiveElements : PlaceholderCharacterModel
{
    public override string CustomTrailPath
    {
        get => SceneHelper.GetScenePath("vfx/card_trail_" + this.PlaceholderID);
    }
    
    
    
    public override Color EnergyLabelOutlineColor => new Color("0000007F");
    public override string CustomEnergyCounterPath
    {
        get => SceneHelper.GetScenePath($"combat/energy_counters/sage_energy_counter");
    }
    
    
    // Utilisation d'un HashSet pour éviter les doublons d'éléments
    public static HashSet<CardElementTag> Echo = new() { CardElementTag.Neutral };

    public static int GetEchoStateForDescription()
    {
        if (Echo.Count == 6) return 6; //echo has all element
        return (int) Echo.LastOrDefault(); //echo has only one element
    }
    
    // Méthode utilitaire pour changer l'écho facilement
    public static void SetEcho(params CardElementTag[] elements)
    {
        Echo.Clear();
        foreach (var e in elements) Echo.Add(e);
    }
    
    public static void ResetEcho()
    {
        Echo.Clear();
        Echo.Add(CardElementTag.Neutral);
    }
    
    // Méthode utilitaire pour changer l'écho facilement
    public static void SetEchoToAllElements()
    {
        Echo.Clear();
        foreach (var element in Enum.GetValues<CardElementTag>())
        {
            Echo.Add(element);
        }
    }
    
    // this change the placeholder stuff
    public override string PlaceholderID => "silent";
    
    public const string CharacterId = "FiveElements";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 75;
    
    //deck with random starting point in the strike n defend, so the upgrading event won't always upgrade the same element
    // it will still upgrade both card of a same element thought
    //todo when strike are added by event their are always the same elem..
    public override IEnumerable<FiveElementsCard> StartingDeck
    {
        get
        {
            var strikeGroup = new List<FiveElementsCard>
            {
                ModelDb.Card<WaterStrike>(),
                ModelDb.Card<WoodStrike>(),
                ModelDb.Card<FireStrike>(),
                ModelDb.Card<EarthStrike>(),
                ModelDb.Card<MetalStrike>(),
            };
            var defendGroup = new List<FiveElementsCard>
            {
                ModelDb.Card<WaterDefend>(),
                ModelDb.Card<WoodDefend>(),
                ModelDb.Card<FireDefend>(),
                ModelDb.Card<EarthDefend>(),
                ModelDb.Card<MetalDefend>(),
            };
                

            // todo? find better rng here
            // Si tu ne l'as pas sous la main, Rng.Chaotic est l'alternative
            int offset = Rng.Chaotic.NextInt(0, 5);
            
            // 3. Faire tourner les groupes (Rotation circulaire)
            // On prend à partir de l'offset, puis on ajoute ce qu'on a sauté
            var rotatedStrike = strikeGroup.Skip(offset).Concat(strikeGroup.Take(offset));
            var rotatedDefend = defendGroup.Skip(offset).Concat(defendGroup.Take(offset));

            // 4. Construire le deck final
            var finalDeck = new List<FiveElementsCard>();
            finalDeck.AddRange(rotatedStrike);
            finalDeck.AddRange(rotatedDefend);
            finalDeck.Add(ModelDb.Card<Activation>());
            
            /*
            //WATER
            finalDeck.Add(ModelDb.Card<WaterCreation>());   //done
            finalDeck.Add(ModelDb.Card<WaterBubble>());     //done
            finalDeck.Add(ModelDb.Card<WaterMark>());       //done
            finalDeck.Add(ModelDb.Card<WaterSpirit>());     //done
            finalDeck.Add(ModelDb.Card<WaterCall>());       //done
            finalDeck.Add(ModelDb.Card<WaterLord>());       //done
            finalDeck.Add(ModelDb.Card<WaterShell>());      //done
            finalDeck.Add(ModelDb.Card<WaterFlow>());       //done
            finalDeck.Add(ModelDb.Card<WaterTyphoon>());    //done
            finalDeck.Add(ModelDb.Card<WaterTide>());       //done
            finalDeck.Add(ModelDb.Card<WaterTsunami>());    //done
            finalDeck.Add(ModelDb.Card<WaterVeil>());       //done
            finalDeck.Add(ModelDb.Card<WaterCanon>());      //done
            
            finalDeck.Add(ModelDb.Card<WaterDrop>());       //done
            */
            
            /*
            //WOOD
            finalDeck.Add(ModelDb.Card<WoodCreation>());    //done
            finalDeck.Add(ModelDb.Card<Wood>(WoodBark));    //done it currently scale down with strength too
            finalDeck.Add(ModelDb.Card<WoodFangs>());       //done way of doubling damage to block might be wrong
            finalDeck.Add(ModelDb.Card<WoodSpirit>());      //done
            finalDeck.Add(ModelDb.Card<WoodLash>());        //done
            finalDeck.Add(ModelDb.Card<WoodClaws>());       //done works but need to see what happen if it gains replay
            finalDeck.Add(ModelDb.Card<WoodSeed>());        //done
            finalDeck.Add(ModelDb.Card<WoodLeaf>());        //done
            finalDeck.Add(ModelDb.Card<WoodMark>());        //done
            finalDeck.Add(ModelDb.Card<WoodQueen>());       //done
            finalDeck.Add(ModelDb.Card<WoodFury>());        //done maybe upgrade should give +hit instead
            finalDeck.Add(ModelDb.Card<WoodSurge>());       //done 1 attack for every strength might be a bit absurd?
            finalDeck.Add(ModelDb.Card<WoodRoots>());       //done
            */
            
            /*
            //FIRE
            finalDeck.Add(ModelDb.Card<FireCreation>());    //done
            finalDeck.Add(ModelDb.Card<FireNova>());        //done
            finalDeck.Add(ModelDb.Card<FireTouch>());       //done
            finalDeck.Add(ModelDb.Card<FireSpirit>());      //done
            finalDeck.Add(ModelDb.Card<FireRise>());        //done
            finalDeck.Add(ModelDb.Card<FireWeaving>());     //done is it good?
            finalDeck.Add(ModelDb.Card<FireStorm>());       //done
            finalDeck.Add(ModelDb.Card<FireForce>());       //done
            finalDeck.Add(ModelDb.Card<FireEater>());       //done need to do smthing when exhaust nothing ?
            finalDeck.Add(ModelDb.Card<FireWings>());       //done
            finalDeck.Add(ModelDb.Card<FireBall>());        //done
            finalDeck.Add(ModelDb.Card<FireDance>());       //done is it good to be rare?
            finalDeck.Add(ModelDb.Card<FireBlossom>());     //done
            
            finalDeck.Add(ModelDb.Card<FirePlume>());       //done maybe increase incendesence damage?
            */
            
            /*
            //EARTH
            finalDeck.Add(ModelDb.Card<EarthCreation>());   //done
            finalDeck.Add(ModelDb.Card<EarthWard>());       //done
            finalDeck.Add(ModelDb.Card<EarthShield>());     //done  maybe change how it work to only apply block once
            finalDeck.Add(ModelDb.Card<EarthSpirit>());     //done
            finalDeck.Add(ModelDb.Card<EarthBlast>());      //done
            finalDeck.Add(ModelDb.Card<EarthJewel>());      //done
            finalDeck.Add(ModelDb.Card<EarthWall>());       //done
            finalDeck.Add(ModelDb.Card<EarthCrown>());      //done clunky, give dex before card being played so you don't see the real block value ganied on the card and if it give after it's useless
            finalDeck.Add(ModelDb.Card<EarthPlate>());      //done
            finalDeck.Add(ModelDb.Card<EarthBorn>());       //done
            finalDeck.Add(ModelDb.Card<EarthGuardian>());   //done
            finalDeck.Add(ModelDb.Card<EarthQuake>());      //done (maybe remove the block gain on replayed card)
            finalDeck.Add(ModelDb.Card<EarthMagma>());      //done
            */
            
            /*
            //METAL
            finalDeck.Add(ModelDb.Card<MetalCreation>());   //done
            finalDeck.Add(ModelDb.Card<MetalBlade>());      //done
            finalDeck.Add(ModelDb.Card<MetalMark>());       //done
            finalDeck.Add(ModelDb.Card<MetalSpirit>());     //done
            finalDeck.Add(ModelDb.Card<MetalPounce>());     //done
            finalDeck.Add(ModelDb.Card<MetalChains>());     //done
            finalDeck.Add(ModelDb.Card<MetalRush>());       //done
            finalDeck.Add(ModelDb.Card<MetalForge>());      //done maybe show how many card will be upgraded? and count itself ?
            finalDeck.Add(ModelDb.Card<MetalEdge>());       //done
            finalDeck.Add(ModelDb.Card<MetalMettle>());     //done
            finalDeck.Add(ModelDb.Card<MetalSlash>());      //done
            finalDeck.Add(ModelDb.Card<MetalRefinement>()); //done
            finalDeck.Add(ModelDb.Card<MetalCore>());       //done
            */
            
            /*
            //NEUTRAL
            finalDeck.Add(ModelDb.Card<Cycle>());           //done
            finalDeck.Add(ModelDb.Card<Distortion>());      //done
            finalDeck.Add(ModelDb.Card<Isolation>());       //done
            finalDeck.Add(ModelDb.Card<MoreOrLess>());      //done maybe don't count card from exhaust pile
            finalDeck.Add(ModelDb.Card<Absorption>());      //done
            finalDeck.Add(ModelDb.Card<Incantation>());     //done
            finalDeck.Add(ModelDb.Card<Decimation>());      //done
            finalDeck.Add(ModelDb.Card<Meditation>());      //done could not filter draw, had to change it a bit, don't like it it's weird
            finalDeck.Add(ModelDb.Card<Circulation>());     //done maybe make it cost 0(or refun itself) if it can't select anything
            finalDeck.Add(ModelDb.Card<Recycle>());         //done
            finalDeck.Add(ModelDb.Card<UltimateForm>());    //done
            finalDeck.Add(ModelDb.Card<AllOrOne>());        //done make it choose between the 2 effects ?
            finalDeck.Add(ModelDb.Card<EchoFormation>());   //done maybe make it stackable ??
            finalDeck.Add(ModelDb.Card<Annihilation>());    //done
            finalDeck.Add(ModelDb.Card<Domination>());      //done maybe make it cost 0(or refun itself) if it can't select anything
            
            finalDeck.Add(ModelDb.Card<Fulu>());            //done maybe go back to fulu transforming and give small buff on each fulu?
            finalDeck.Add(ModelDb.Card<More>());            //done
            finalDeck.Add(ModelDb.Card<Less>());            //done
            
            //water fulu 1or2 wave, wood fulu 1 temp str(or 2dmg?), fire fulu apply 2-3 burn, earth fulu 3 block, metal fulu 1 vigor
            
            //multi
            finalDeck.Add(ModelDb.Card<>()); 
            finalDeck.Add(ModelDb.Card<>()); 
            //ancient
            finalDeck.Add(ModelDb.Card<>()); 
            finalDeck.Add(ModelDb.Card<SpiritsForm>());     //done but maybe not yet working for all stuff that say foreach fire/etc card played this turn
            
            */
            return finalDeck;
        }
    }
    

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<Relic1>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<FiveElementsCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<FiveElementsRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<FiveElementsPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}