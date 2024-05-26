using UnityEngine;

namespace TeleCore.UI;

public static class TUI
{
    public static void DoControl(Rect rect)
    {
        var id = GUIUtility.GetControlID(FocusType.Passive, rect);
        
    }
}