using BaseLib.Abstracts;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Character;

namespace FiveElements.FiveElementsCode.Potions;

[Pool(typeof(FiveElementsPotionPool))]
public abstract class FiveElementsPotion : CustomPotionModel;