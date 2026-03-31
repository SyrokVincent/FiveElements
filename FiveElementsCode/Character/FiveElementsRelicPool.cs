using BaseLib.Abstracts;
using FiveElements.FiveElementsCode.Extensions;
using Godot;

namespace FiveElements.FiveElementsCode.Character;

public class FiveElementsRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => FiveElements.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}