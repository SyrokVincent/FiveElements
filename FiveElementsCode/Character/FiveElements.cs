using BaseLib.Abstracts;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Cards._2_Common;
using FiveElements.FiveElementsCode.Cards._3_Uncommon;
using FiveElements.FiveElementsCode.Cards._4_Rare;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Cards._6_Ancient;
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
        get => SceneHelper.GetScenePath("combat/card_trail/card_trail_sage");
    }
    
    public override Color EnergyLabelOutlineColor => new Color("0000007F");
    public override string CustomEnergyCounterPath
    {
        get => SceneHelper.GetScenePath($"combat/energy_counters/sage_energy_counter");
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
            //water feel really cool to play right now, might be a bit too strong ?
            
            finalDeck.Add(ModelDb.Card<WaterCreation>());   //removed
            finalDeck.Add(ModelDb.Card<WaterSource>());     //done
            finalDeck.Add(ModelDb.Card<WaterBubble>());     //done too strong ?
            finalDeck.Add(ModelDb.Card<WaterMark>());       //done
            finalDeck.Add(ModelDb.Card<WaterSpirit>());     //done
            finalDeck.Add(ModelDb.Card<WaterCall>());       //done
            finalDeck.Add(ModelDb.Card<WaterLord>());       //done
            finalDeck.Add(ModelDb.Card<WaterShell>());      //done weird effect ?
            finalDeck.Add(ModelDb.Card<WaterFlow>());       //done
            finalDeck.Add(ModelDb.Card<WaterTyphoon>());    //done
            finalDeck.Add(ModelDb.Card<WaterTide>());       //done
            finalDeck.Add(ModelDb.Card<WaterTsunami>());    //done
            finalDeck.Add(ModelDb.Card<WaterVeil>());       //done weird effect ?
            finalDeck.Add(ModelDb.Card<WaterCanon>());      //done
            
            finalDeck.Add(ModelDb.Card<WaterDrop>());       //done
            */
            
            /*
            //WOOD
            finalDeck.Add(ModelDb.Card<WoodCreation>());    //removed
            finalDeck.Add(ModelDb.Card<WoodChop>());        //done
            finalDeck.Add(ModelDb.Card<WoodBark>());        //done it currently scale down with strength too
            finalDeck.Add(ModelDb.Card<WoodFangs>());       //done way of doubling damage to block might be wrong
            finalDeck.Add(ModelDb.Card<WoodSpirit>());      //done
            finalDeck.Add(ModelDb.Card<WoodLash>());        //done
            finalDeck.Add(ModelDb.Card<WoodClaws>());       //done
            finalDeck.Add(ModelDb.Card<WoodSeed>());        //done
            finalDeck.Add(ModelDb.Card<WoodLeaf>());        //done
            finalDeck.Add(ModelDb.Card<WoodMark>());        //done draw on this too strong ?
            finalDeck.Add(ModelDb.Card<WoodQueen>());       //done
            finalDeck.Add(ModelDb.Card<WoodFury>());        //done
            finalDeck.Add(ModelDb.Card<WoodSurge>());       //done might be too strong
            finalDeck.Add(ModelDb.Card<WoodRoots>());       //done
            */
            
            /*
            //FIRE
            finalDeck.Add(ModelDb.Card<FireCreation>());    //removed
            finalDeck.Add(ModelDb.Card<FireProtection>());  //done
            finalDeck.Add(ModelDb.Card<FireNova>());        //done
            finalDeck.Add(ModelDb.Card<FireTouch>());       //done
            finalDeck.Add(ModelDb.Card<FireSpirit>());      //done
            finalDeck.Add(ModelDb.Card<FireRise>());        //done
            finalDeck.Add(ModelDb.Card<FireWeaving>());     //done excellent in niche case (remove ethereal?)
            finalDeck.Add(ModelDb.Card<FireStorm>());       //done
            finalDeck.Add(ModelDb.Card<FireForce>());       //done
            finalDeck.Add(ModelDb.Card<FireEater>());       //done need to do smthing when exhaust nothing ?(remove ethereal?)
            finalDeck.Add(ModelDb.Card<FireWings>());       //done
            finalDeck.Add(ModelDb.Card<FireBall>());        //done
            finalDeck.Add(ModelDb.Card<FireDance>());       //done is it good enough to be rare?
            finalDeck.Add(ModelDb.Card<FireBlossom>());     //done
            
            finalDeck.Add(ModelDb.Card<FirePlume>());       //done maybe increase incendescence damage?
            */
            
            /*
            //EARTH
            finalDeck.Add(ModelDb.Card<EarthCreation>());   //removed
            finalDeck.Add(ModelDb.Card<EarthFoundation>()); //done
            finalDeck.Add(ModelDb.Card<EarthWard>());       //done
            finalDeck.Add(ModelDb.Card<EarthJewel>());      //done
            finalDeck.Add(ModelDb.Card<EarthSpirit>());     //done
            finalDeck.Add(ModelDb.Card<EarthGuardian>());   //done
            finalDeck.Add(ModelDb.Card<EarthBlast>());      //done
            finalDeck.Add(ModelDb.Card<EarthWall>());       //done
            finalDeck.Add(ModelDb.Card<EarthCrown>());      //done 
            finalDeck.Add(ModelDb.Card<EarthPlate>());      //done
            finalDeck.Add(ModelDb.Card<EarthBorn>());       //done
            finalDeck.Add(ModelDb.Card<EarthQuake>());      //done (maybe remove the block gain on replayed card)
            finalDeck.Add(ModelDb.Card<EarthClay>());       //done
            finalDeck.Add(ModelDb.Card<EarthMagma>());      //done
            
            finalDeck.Add(ModelDb.Card<EarthWarrior>());    //done
            */
            
            /*
            //METAL
            finalDeck.Add(ModelDb.Card<MetalCreation>());   //removed
            finalDeck.Add(ModelDb.Card<MetalMuscle>());     //done
            finalDeck.Add(ModelDb.Card<MetalBlade>());      //done
            finalDeck.Add(ModelDb.Card<MetalMark>());       //done
            finalDeck.Add(ModelDb.Card<MetalSpirit>());     //done
            finalDeck.Add(ModelDb.Card<MetalPounce>());     //done
            finalDeck.Add(ModelDb.Card<MetalChains>());     //done
            finalDeck.Add(ModelDb.Card<MetalRush>());       //done don't like it too much need better and less forced metal essence payof
            finalDeck.Add(ModelDb.Card<MetalForge>());      //done
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
            finalDeck.Add(ModelDb.Card<Meditation>());      //done 
            finalDeck.Add(ModelDb.Card<Circulation>());     //done can't select power? maybe make it cost 0(or refund itself) if it can't select anything
            finalDeck.Add(ModelDb.Card<Recycle>());         //done maybe remove retain and add shift?
            finalDeck.Add(ModelDb.Card<UltimateForm>());    //done
            finalDeck.Add(ModelDb.Card<AllOrOne>());        //done
            finalDeck.Add(ModelDb.Card<EchoFormation>());   //done maybe make it stackable ??
            finalDeck.Add(ModelDb.Card<Annihilation>());    //done
            finalDeck.Add(ModelDb.Card<Domination>());      //done can't select power? add back exhaust ? and maybe make it cost 0(or refun itself) if it can't select anything
            
            finalDeck.Add(ModelDb.Card<Fulu>());            //done
            finalDeck.Add(ModelDb.Card<More>());            //done
            finalDeck.Add(ModelDb.Card<Less>());            //done
            
            //water fulu 1or2 wave, wood fulu 1 temp str(or 2dmg?), fire fulu apply 2-3 burn, earth fulu 3 block, metal fulu 1 vigor
            
            //multi
            finalDeck.Add(ModelDb.Card<MindAndBodyAttunement>());   //done no idea how balance it is
            finalDeck.Add(ModelDb.Card<MultiActivation>());         //done no idea how balance it is
            //ancient
            finalDeck.Add(ModelDb.Card<Incarnation>());     //done
            finalDeck.Add(ModelDb.Card<SpiritsForm>());     //done
            
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