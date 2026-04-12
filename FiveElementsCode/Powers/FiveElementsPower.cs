using BaseLib.Abstracts;
using BaseLib.Extensions;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Powers;

public abstract class FiveElementsPower : CustomPowerModel
{
    
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
    
    //Loads from FiveElements/images/powers/your_power.png
    public override string CustomPackedIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}