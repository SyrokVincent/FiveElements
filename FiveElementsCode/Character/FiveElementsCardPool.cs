using BaseLib.Abstracts;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;

namespace FiveElements.FiveElementsCode.Character;

public class FiveElementsCardPool : CustomCardPoolModel
{
    
    // Variables de stockage (Backing fields) avec valeurs par défaut
    private string _currentTextEnergy = "charui/text_energy_five_elements.png";
    private Color _currentDeckColor = new("ffffff");

    public override string Title => FiveElements.CharacterId;//This is not a display name.

    // Utilisation de l'expression-bodied member pour lire la variable
    public override string BigEnergyIconPath => GetEnergyPath(Character.FiveElements.Echo).ImagePath();
    public override string TextEnergyIconPath => _currentTextEnergy.ImagePath();
    
    // Logique de sélection de l'image
    private string GetEnergyPath(CardElementTag echo) => echo switch
    {
        CardElementTag.Water => "charui/big_energy_five_elements_water.png",
        CardElementTag.Wood  => "charui/big_energy_five_elements_wood.png",
        CardElementTag.Fire  => "charui/big_energy_five_elements_fire.png",
        CardElementTag.Earth => "charui/big_energy_five_elements_earth.png",
        CardElementTag.Metal => "charui/big_energy_five_elements_metal.png",
        _                    => "charui/big_energy_five_elements.png" // Valeur par défaut
    };
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