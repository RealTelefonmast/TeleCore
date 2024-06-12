using System;
using UnityEngine;

namespace TeleCore.DGUI;

public interface IDrawable
{
    void Draw();
}

public interface IUIElement
{
    public Vector2 Position { get; internal set; }
    public Vector2 Size { get; internal set; }
    public Rect Rect { get; internal set; }
}

public interface IButton
{
    public Action Action { get; internal set; }
}

public struct UIElement : IUIElement
{
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Rect Rect { get; set; }
}

public struct ButtonElement : IUIElement, IButton
{
    private UIElement _base;

    public Action Action { get; set; }

    public Vector2 Position
    {
        get => _base.Position;
        set => _base.Position = value;
    }

    public Vector2 Size
    {
        get => _base.Size;
        set => _base.Size = value;
    }

    public Rect Rect
    {
        get => _base.Rect;
        set => _base.Rect = value;
    }
}

public static class DGUI
{
    public static IUIElement At(this IUIElement e, Vector2 pos)
    {
        e.Position = pos;
        e.Rect = new Rect(pos, e.Size);
        return e;
    }

    public static IUIElement WithSize(this IUIElement e, Vector2 size)
    {
        e.Size = size;
        e.Rect = new Rect(e.Position, size);
        return e;
    }

    public static IButton WithAction(this IButton b, Action action)
    {
        b.Action = action;
        return b;
    }
}