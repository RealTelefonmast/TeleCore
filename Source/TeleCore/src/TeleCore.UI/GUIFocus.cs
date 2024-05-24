using System;
using UnityEngine;

namespace TeleCore.UI;

public class GUIFocus : IDisposable
{
    private Rect focusRect;
    private string controlName;

    public GUIFocus(Rect focusRect, string controlName)
    {
        this.focusRect = focusRect;
        this.controlName = controlName;
        GUI.SetNextControlName(controlName);
    }
    
    public void Dispose()
    {
        var clicked = Event.current.isMouse && Event.current.button == 0;
        if (clicked && !focusRect.Contains(Event.current.mousePosition))
        {
            GUI.FocusControl(null);
        }
    }
}