using System.Collections.Generic;

namespace TeleCore.Animations;

public static class DGUIReferences<T> where T : IUIElement
{
    private static Dictionary<IUIReferences<T>, HashSet<T>> _references;

    static DGUIReferences()
    {
        _references = new Dictionary<IUIReferences<T>, HashSet<T>>();
    }
    
    public static void RegisterElement(IUIReferences<T> references, T element)
    {
        if (_references.ContainsKey(references))
        {
            _references[references].Add(element);
        }
        else
        {
            _references[references] = [element];
        }
    }

    public static IReadOnlyCollection<T> GetHeldElements(IUIReferences<T> references)
    {
        if (_references.TryGetValue(references, out var elements))
        {
            return elements;
        }
        return new List<T>();
    }

    public static T GetElement(IUIReferences<T> references, int id)
    {
        if (_references.ContainsKey(references))
        {
            foreach (var element in _references[references])
            {
                if (element.ID == id)
                {
                    return element;
                }
            }
        }
        return default(T);
    }
}