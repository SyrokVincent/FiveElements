using BaseLib.Abstracts;
using BaseLib.Extensions;
using FiveElements.FiveElementsCode.Cards._2_Common;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Powers;

public class WoodCreationStrengthPower : TemporaryStrengthPower, ICustomPower
{
 

    
    // Indique que le pouvoir provient de la carte WoodCreation
    public override AbstractModel OriginModel => ModelDb.Card<WoodCreation>();

    protected override bool IsPositive => true;
    
    public string CustomPackedIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public string CustomBigIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}