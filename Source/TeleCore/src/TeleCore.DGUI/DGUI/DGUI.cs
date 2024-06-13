using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.DGUI;

public enum Alignment
{
    Horizontal,
    Vertical
}

public interface IUIBase
{
    int ID { get; set; }
    Rect Rect { get; set; }
}

public interface IUIContainer : IUIBase
{
    
}

public interface IButton : IUIBase
{
    public bool IsToggle { get; set; }
    public bool IsPressed { get; set; }
}

public interface IStackRow : IUIContainer
{
    
}


public struct UIBase
{
    public int ID { get; set; }
}

public struct Button
{
    
}

public struct StackRow
{
    private Rect _rect;
    private Alignment _alignment;
    

    internal StackRow(Rect rect, Alignment alignment)
    {
        _rect = rect;
        _alignment = alignment;
    }
}

public static partial class DGUI
{
    private static readonly List<string> StringCache = new();
    
    //Does not guarantee uniqueness
    public static int CacheString(string str)
    {
        var id = StringCache.Count;
        StringCache.Add(str);
        return id;
    }

    public static string StringById(int id)
    {
        return StringCache[id];
    }
}

public struct stringid(string str)
{
    private int _id = DGUI.CacheString(str);
    
    public string Value => DGUI.StringById(_id);

    public static implicit operator stringid(string str)
    {
        return new stringid(str);
    }
}

public static partial class DGUI
{
    public static Button Button(Rect rect)
    {
        
    }
    
    
}

public static partial class DGUI
{
    public static StackRow StackRow(Rect rect, Alignment alignment = Alignment.Horizontal)
    {
        var stack = new StackRow(rect, alignment);
        return stack;
    }
    
    public static StackRow Add(this StackRow row, IUIBase element)
    {
        return row;
    }
}

public static partial class DGUI
{
    public static void Build()
    {
        
    }
}

public static class TestStuffAgain
{
    public static void Test()
    {
        var rowRect = new Rect();
        var button = DGUI.Button();
        var row = DGUI.StackRow(rowRect).Add(button);
    }
    
    
}