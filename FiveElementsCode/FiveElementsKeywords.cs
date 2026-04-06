using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using static FiveElements.FiveElementsCode.FiveElementsKeywords;

namespace FiveElements.FiveElementsCode;

public static class FiveElementsKeywords
{
    
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Essence;
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Shift;
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Echo;
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Generate;
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Water;
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Wood;
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Fire;
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Earth;
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Metal;

    
    public static bool IsShift(this CardModel card)
    {
        return card.Keywords.Contains(Shift);
    }
    
    private static readonly LocString _period = new LocString("card_keywords", "PERIOD");

    public static string GetLocKeyPrefix(this CardKeyword keyword)
    {
        if (keyword == Essence)  return "FIVEELEMENTS-ESSENCE";
        if (keyword == Shift)  return "FIVEELEMENTS-SHIFT";
        if (keyword == Echo)   return "FIVEELEMENTS-ECHO";
        if (keyword == Water)  return "FIVEELEMENTS-WATER";
        if (keyword == Wood)   return "FIVEELEMENTS-WOOD";
        if (keyword == Fire)   return "FIVEELEMENTS-FIRE";
        if (keyword == Earth)  return "FIVEELEMENTS-EARTH";
        if (keyword == Metal)  return "FIVEELEMENTS-METAL";
        if (keyword == Generate) return "FIVEELEMENTS-GENERATE";
    
        // Si le nom est nul (ça arrive si c'est un keyword dynamique), on met une sécurité
        string name = Enum.GetName(typeof(CardKeyword), keyword);
        if (string.IsNullOrEmpty(name)) return keyword.ToString();
        return StringHelper.Slugify(name);
    }

    public static LocString GetTitle(this CardKeyword keyword)
    {
        return new LocString("card_keywords", keyword.GetLocKeyPrefix() + ".title");
    }

    public static LocString GetDescription(this CardKeyword keyword)
    {
        return new LocString("card_keywords", keyword.GetLocKeyPrefix() + ".description");
    }

    public static string GetCardText(this CardKeyword keyword)
    {
        return $"[gold]{keyword.GetTitle().GetFormattedText()}[/gold]{_period.GetRawText()}";
    }
}