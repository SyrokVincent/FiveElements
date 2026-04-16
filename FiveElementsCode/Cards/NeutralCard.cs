using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class NeutralCard : FiveElementsCard
{
    protected NeutralCard(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target)
    {
        CanonicalElementTags = [CardElementTag.Neutral];
    }
    
    public override IEnumerable<CardElementTag> ElementTags 
    {
        get 
        {
            // 1. Vérifie si la carte a le Keyword Attune
            if (Keywords.Contains(FiveElementsKeywords.Attune))
            {
                return Character.FiveElements.Echo;
            }
            // 1. Vérifie si la carte a le Keyword Shift
            if (Keywords.Contains(FiveElementsKeywords.Shift))
            {
                // return what echo generate
            
                HashSet<CardElementTag> newEcho = new(){ CardElementTag.Neutral };
                if (Character.FiveElements.Echo.Contains(CardElementTag.Water)) newEcho.Add(CardElementTag.Wood);
                if (Character.FiveElements.Echo.Contains(CardElementTag.Wood)) newEcho.Add(CardElementTag.Fire);
                if (Character.FiveElements.Echo.Contains(CardElementTag.Fire)) newEcho.Add(CardElementTag.Earth);
                if (Character.FiveElements.Echo.Contains(CardElementTag.Earth)) newEcho.Add(CardElementTag.Metal);
                if (Character.FiveElements.Echo.Contains(CardElementTag.Metal)) newEcho.Add(CardElementTag.Water);
                
                return newEcho;
            }

            // 2. Si pas de Attune ni Shift, on utilise le comportement de base de FiveElementsCard
            return base.ElementTags;
        }
    }
}
