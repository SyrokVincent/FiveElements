using BaseLib.Abstracts;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards.Basic;
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

namespace FiveElements.FiveElementsCode.Character;

  
  
public class FiveElements : PlaceholderCharacterModel
{
    
    public const string CharacterId = "FiveElements";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<FiveElementsCard> StartingDeck =>
    [
        ModelDb.Card<WaterStrike>(),
        ModelDb.Card<WoodStrike>(),
        ModelDb.Card<FireStrike>(),
        ModelDb.Card<EarthStrike>(),
        ModelDb.Card<MetalStrike>(),
        ModelDb.Card<WaterDefend>(),
        ModelDb.Card<WoodDefend>(),
        ModelDb.Card<FireDefend>(),
        ModelDb.Card<EarthDefend>(),
        ModelDb.Card<MetalDefend>(),
        ModelDb.Card<Creation>(),
        ModelDb.Card<Activation>()
    ];

    

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