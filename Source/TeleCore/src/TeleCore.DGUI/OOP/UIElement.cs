using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.DGUI.OOP;

public class UIElement
{
    private Rect _rect;

    public virtual void OnLayout()
    {
        
    }

    public virtual void ProcessInput()
    {
        
    }

    public virtual void OnRepaint()
    {
        
    }
}

public class UIContainer
{
    private Rect _rect;
    private List<UIElement> _children;

    public void Draw()
    {
        //Layout
        foreach (var child in _children)
        {
            child.OnLayout();
        }
        
        //Inputs
        
        //Repaint
    }
}