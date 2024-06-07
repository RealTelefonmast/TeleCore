using System;
using UnityEngine;

namespace TeleCore.Animations;

public enum StyleContextType
{
    Box,
    TextField,
    //TODO: Add more
}

public enum StyleType
{
    Normal,
    Hovered,
    Pressed,
    Disabled,
    //TODO: Add more
}

public struct StyleChanges
{
    public Texture2D background;
    
}

public struct GUIStyleContext : IDisposable
{
    private StyleContextType _context;
    private GUIStyle _style;
    
    //TODO: Have better cache of old state
    private Texture2D oldBackground;

    public GUIStyleContext(StyleContextType type)
    {
        _context = type;
        _style = ForContext(_context);
    }
    
    //TODO: Remove this temporary for box ctor
    public GUIStyleContext(StyleContextType type, Texture2D background)
    {
        _context = type;
        _style = ForContext(_context);
        oldBackground = _style.normal.background;
        _style.normal.background = background;
    }

    public static GUIStyle ForContext(StyleContextType context)
    {
        return context switch
        {
            StyleContextType.Box => GUI.skin.box,
            StyleContextType.TextField => GUI.skin.textField,
            _ => throw new ArgumentOutOfRangeException(nameof(context), context, null)
        };
    }
    
    public void Dispose()
    {
        switch (_context)
        {
            case StyleContextType.Box:
                _style.normal.background = oldBackground;
                break;
            case StyleContextType.TextField:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _style = null;
        oldBackground = null;
        _context = 0;
    }
}