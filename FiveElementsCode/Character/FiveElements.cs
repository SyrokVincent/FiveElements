using BaseLib.Abstracts;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards.Basic;
using FiveElements.FiveElementsCode.Cards.Common;
using FiveElements.FiveElementsCode.Cards.Rare;
using FiveElements.FiveElementsCode.Cards.Token;
using FiveElements.FiveElementsCode.Cards.Uncommon;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;

namespace FiveElements.FiveElementsCode.Character;

  
  
public class FiveElements : PlaceholderCharacterModel
{
    
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
            finalDeck.Add(ModelDb.Card<WaterFlow>());
            finalDeck.Add(ModelDb.Card<WaterBubble>());
            finalDeck.Add(ModelDb.Card<WaterSpirit>());
            finalDeck.Add(ModelDb.Card<WaterLord>());
            finalDeck.Add(ModelDb.Card<WaterShell>());
            finalDeck.Add(ModelDb.Card<WaterTsunami>());
            finalDeck.Add(ModelDb.Card<WaterCanon>());
            finalDeck.Add(ModelDb.Card<WaterVeil>()); 
            finalDeck.Add(ModelDb.Card<WaterVeil>()); 
            finalDeck.Add(ModelDb.Card<WaterVeil>()); 
            finalDeck.Add(ModelDb.Card<WaterVeil>()); 

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