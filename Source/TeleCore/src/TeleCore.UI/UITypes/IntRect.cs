using Unity.Mathematics;
using UnityEngine;

namespace TeleCore.TeleUI.UITypes;

/// <summary>
/// This Rect helps use UI by-pixel and avoid weird unrounded artifacts.
/// </summary>
public struct IntRect
{
    private int _x;
    private int _y;
    private int _width;
    private int _height;
    
    public int x => _x;
    public int y => _y;
    public int width => _width;
    public int height => _height;
    
    public IntRect(Rect rect)
    {
        _x = (int)rect.x;
        _y = (int)rect.y;
        _width = (int)rect.width;
        _height = (int)rect.height;
    }
    
    public IntRect(int x, int y, int width, int height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
    }
    
    public IntRect(float x, float y, float width, float height) : this((int)x, (int)y, (int)width, (int)height)
    {
    }

    public IntRect SizeBy(int2 size)
    {
        _x += size.x;
        _y += size.y;
        _width += size.x * 2;
        _height += size.y * 2;
        return this;
    }
}