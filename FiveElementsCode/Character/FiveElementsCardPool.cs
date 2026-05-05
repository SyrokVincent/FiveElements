using BaseLib.Abstracts;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;

namespace FiveElements.FiveElementsCode.Character;

public class FiveElementsCardPool : CustomCardPoolModel
{
    
    private string _currentEnergy = "charui/energy/big_energy_sage_neutral.png";
    private string _currentTextEnergy = "charui/energy/text_energy_five_elements.png";
    private Color _currentDeckColor = new("ffffff");

    public override string Title => FiveElements.CharacterId;//This is not a display name.

    // default path for hovertip and such
    public override string BigEnergyIconPath => _currentEnergy.ImagePath();
    public override string TextEnergyIconPath => _currentTextEnergy.ImagePath();
    
    // Logique de sélection de l'image pour l'icone des carte
    public string GetEnergyPathForCard(HashSet<CardElementTag> echo)
    {
        if (echo.Contains(CardElementTag.Neutral))
        {
            if (echo.Count < 6) return "charui/energy/big_energy_sage_neutral.png";
            return "charui/energy/big_energy_five_elements_all.png";
        }
        if (echo.Contains(CardElementTag.Water)) return "charui/energy/big_energy_sage_water.png";
        if (echo.Contains(CardElementTag.Wood)) return "charui/energy/big_energy_sage_wood.png";
        if (echo.Contains(CardElementTag.Fire)) return "charui/energy/big_energy_sage_fire.png";
        if (echo.Contains(CardElementTag.Earth)) return "charui/energy/big_energy_sage_earth.png";
        if (echo.Contains(CardElementTag.Metal)) return "charui/energy/big_energy_sage_metal.png";
        return "charui/energy/big_energy_sage_neutral.png";
    }
    
    
    //Color of small card icons
    public override Color DeckEntryCardColor => _currentDeckColor;
    
    

    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    public override float H => 1f; //Hue; changes the color.
    public override float S => 0f; //Saturation
    public override float V => 0.6f; //Brightness

    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load FiveElements/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/


    public override bool IsColorless => false;
}