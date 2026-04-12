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
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;

namespace FiveElements.FiveElementsCode.Character;

  
  
public class FiveElements : PlaceholderCharacterModel
{
    public override string CustomTrailPath
    {
        get => SceneHelper.GetScenePath("vfx/card_trail_" + this.PlaceholderID);
    }
    
    //todo make echo an array for if one day some card have multi element
    public static CardElementTag Echo = CardElementTag.Neutral;
    
    // this change the placeholder stuff
    public override string PlaceholderID => "silent";
    
    public const string CharacterId = "FiveElements";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;
    
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
            finalDeck.Add(ModelDb.Card<Creation>());
            finalDeck.Add(ModelDb.Card<Activation>());
            finalDeck.Add(ModelDb.Card<AllOrOne>());
            finalDeck.Add(ModelDb.Card<MetalSpirit>()); 
            finalDeck.Add(ModelDb.Card<MetalRush>());    
            finalDeck.Add(ModelDb.Card<MetalRush>());    
            finalDeck.Add(ModelDb.Card<MetalMettle>()); 
            finalDeck.Add(ModelDb.Card<MetalSlash>());  
            finalDeck.Add(ModelDb.Card<MetalSlash>());  
            finalDeck.Add(ModelDb.Card<MetalSlash>());  
            
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
            */
            
            /*
            //WOOD
            finalDeck.Add(ModelDb.Card<WoodCreation>());     //done
            finalDeck.Add(ModelDb.Card<Wood>());      
            finalDeck.Add(ModelDb.Card<Wood>());
            finalDeck.Add(ModelDb.Card<WoodSpirit>());       //done
            finalDeck.Add(ModelDb.Card<Wood>());
            finalDeck.Add(ModelDb.Card<Wood>());
            finalDeck.Add(ModelDb.Card<Wood>());
            finalDeck.Add(ModelDb.Card<WoodSeed>());        //done
            finalDeck.Add(ModelDb.Card<Wood>());
            finalDeck.Add(ModelDb.Card<Wood>());
            finalDeck.Add(ModelDb.Card<Wood>());
            finalDeck.Add(ModelDb.Card<Wood>());
            finalDeck.Add(ModelDb.Card<Wood>());
            */
            
            /*
            //FIRE
            finalDeck.Add(ModelDb.Card<FireCreation>());    //done
            finalDeck.Add(ModelDb.Card<FireNova>());        //done
            finalDeck.Add(ModelDb.Card<FireTouch>());       //done
            finalDeck.Add(ModelDb.Card<FireSpirit>());      //done
            finalDeck.Add(ModelDb.Card<FireRise>());        //done
            finalDeck.Add(ModelDb.Card<FireWeaving>());     //done
            finalDeck.Add(ModelDb.Card<FireStorm>());       //done
            finalDeck.Add(ModelDb.Card<FireForce>());       //done
            finalDeck.Add(ModelDb.Card<FireEater>());       //done
            finalDeck.Add(ModelDb.Card<FireWings>());       //done
            finalDeck.Add(ModelDb.Card<FireFall>());        //done
            finalDeck.Add(ModelDb.Card<FireDance>());       //done
            finalDeck.Add(ModelDb.Card<FireBlossom>());     //done
            */
            
            /*
            //EARTH
            finalDeck.Add(ModelDb.Card<EarthCreation>());   //done
            finalDeck.Add(ModelDb.Card<Earth>());
            finalDeck.Add(ModelDb.Card<EarthShield>());     //done  maybe change how it work to only apply block once
            finalDeck.Add(ModelDb.Card<EarthSpirit>());     //done
            finalDeck.Add(ModelDb.Card<Earth>());
            finalDeck.Add(ModelDb.Card<EarthJewel>());      //done
            finalDeck.Add(ModelDb.Card<Earth>());
            finalDeck.Add(ModelDb.Card<Earth>());
            finalDeck.Add(ModelDb.Card<Earth>());
            finalDeck.Add(ModelDb.Card<Earth>());
            finalDeck.Add(ModelDb.Card<EarthGuardian>());   //done
            finalDeck.Add(ModelDb.Card<Earth>());
            finalDeck.Add(ModelDb.Card<Earth>());
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
            finalDeck.Add(ModelDb.Card<Metal>());
            finalDeck.Add(ModelDb.Card<MetalMettle>());     //done
            finalDeck.Add(ModelDb.Card<MetalSlash>());      //done
            finalDeck.Add(ModelDb.Card<MetalRefinement>()); //done
            finalDeck.Add(ModelDb.Card<Metal>());
            */
            
            /*
            //NEUTRAL
            finalDeck.Add(ModelDb.Card<Incantation>());     //done
            finalDeck.Add(ModelDb.Card<Isolation>());       //done
            finalDeck.Add(ModelDb.Card<Distortion>());      //done
            finalDeck.Add(ModelDb.Card<Annihilation>());    //done
            finalDeck.Add(ModelDb.Card<Decimation>());      //done
            finalDeck.Add(ModelDb.Card<Circulation>());     //done need upgrade
            finalDeck.Add(ModelDb.Card<AllOrOne>());        //done make it chhose between the 2 effects ?
            finalDeck.Add(ModelDb.Card<Domination>());      //done need upgrade
            finalDeck.Add(ModelDb.Card<MoreOrLess>());      //done maybe don't count card from exhaust pile
            finalDeck.Add(ModelDb.Card<Recycle>());         //done
            finalDeck.Add(ModelDb.Card<Absorption>());      //done
            finalDeck.Add(ModelDb.Card<EchoFormation>());   //done maybe make it stackable ??
            finalDeck.Add(ModelDb.Card<>()); 
            finalDeck.Add(ModelDb.Card<>()); 
            finalDeck.Add(ModelDb.Card<>()); 
            
            //multi
            finalDeck.Add(ModelDb.Card<>()); 
            finalDeck.Add(ModelDb.Card<>()); 
            //ancient
            finalDeck.Add(ModelDb.Card<>()); 
            finalDeck.Add(ModelDb.Card<>()); 
            
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