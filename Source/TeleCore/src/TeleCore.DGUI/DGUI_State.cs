using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.Animations;

public static partial class DGUI
{
    private static Dictionary<int, bool> _buttonStates;

    static DGUI()
    {
        _buttonStates = new Dictionary<int, bool>();
    }
    
    public static bool GetState(int id)
    {
        return _buttonStates.GetValueOrDefault(id, false);
    }

    public static void SetState(int id, bool state = true)
    {
        _buttonStates[id] = state;
    }
    
    internal static int GetId(Rect rect)
    {
        return GUIUtility.GetControlID(rect.GetHashCode(), FocusType.Passive, rect);
    }
}