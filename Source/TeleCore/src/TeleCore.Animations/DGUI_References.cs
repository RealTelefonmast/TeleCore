using System.Collections.Generic;

namespace TeleCore.Animations;

internal static class DGUI_References
{
    private static readonly Dictionary<int, UIBase> LookUp = new Dictionary<int, UIBase>();
    private static readonly Dictionary<int, List<int>> References = new Dictionary<int, List<int>>();
    private static readonly Dictionary<int, int> ReferencesReverse = new Dictionary<int, int>();

    public static UIBase Element(int id)
    {
        if (LookUp.TryGetValue(id, out var element))
        {
            return element;
        }
        return UIBase.Invalid;
    }
    
    public static void Reference(UIBase forElement, IUIBase element)
    {
        //Add new sub-element to parent element
        if (References.ContainsKey(forElement.ID))
        {
            References[forElement.ID].Add(element.ID);
        }
        else
        {
            References[forElement.ID] = new List<int> { element.ID };
        }
        
        //Set new parent element to sub-element
        if (ReferencesReverse.ContainsKey(element.ID))
        {
            ReferencesReverse[element.ID] = forElement.ID;
        }
    }

    public static void Dispose(UIBase element)
    {
        if (ReferencesReverse.ContainsKey(element.ID))
        {
            ReferencesReverse.Remove(element.ID);
        }

        if (References.ContainsKey(element.ID))
        {
            foreach (var subElement in References[element.ID])
            {
                SetEmptyParent(subElement);
            }
            References.Remove(element.ID);
        }
    }

    private static void SetEmptyParent(int forElementID)
    {
        if (ReferencesReverse.ContainsKey(forElementID))
        {
            ReferencesReverse.Remove(forElementID);
        }
    }
}