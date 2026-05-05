using BaseLib.Abstracts;
using FiveElements.FiveElementsCode.Extensions;
using Godot;

namespace FiveElements.FiveElementsCode.Character;

public class FiveElementsPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => FiveElements.Color;


    public override string BigEnergyIconPath => "charui/energy/big_energy_five_elements.png".ImagePath();
    public override string TextEnergyIconPath => "charui/energy/text_energy_five_elements.png".ImagePath();
}