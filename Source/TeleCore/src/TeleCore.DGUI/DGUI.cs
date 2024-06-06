using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Animations;

public struct ElementState
{
    public bool IsHot { get; }
    public bool IsHovered { get; }
    public bool IsDown { get; }
    public bool IsDragging { get; }
}

public enum FocusBehavior
{
    None,
    Hold,
    Click
}

public partial struct UIElementS
{
    private Vector2 _pos;
    private Vector2 _size;
    private Rect _rect;
    private FocusBehavior _focus = FocusBehavior.Hold;
    
    public UIElementS(Rect rect)
    {
        _rect = rect;
        _id = GUIUtility.GetControlID(FocusType.Passive, rect);
    }
    
    private void SetState(EventType evType)
    {
        var isFocused = GUIUtility.hotControl == _id;
        
        switch (evType)
        {
            case EventType.MouseDown:
            {
                var mouseInRect = _rect.Contains(Event.current.mousePosition);
                var mouseDown = Event.current.button == 0 && mouseInRect;
                switch (_focus)
                {
                    case FocusBehavior.Click:
                    {
                        if(GUI.Button(_rect, "", Widgets.EmptyStyle))
                            GUIUtility.hotControl = isFocused ? 0 : _id;
                        break;
                    }
                    case FocusBehavior.Hold:
                    {
                        if(mouseDown)
                            GUIUtility.hotControl = _id;
                        break;
                    }
                }
                break;
            }
            case EventType.MouseUp:
            {
                if (_focus == FocusBehavior.Hold)
                {
                    if (GUIUtility.hotControl == _id)
                        GUIUtility.hotControl = 0;
                }
                break;
            }
            case EventType.MouseDrag:
            {
                if (GUIUtility.hotControl == _id)
                {
                    var newPos = mousePos.x - rect.x;
                    var newSliderVal = Mathf.InverseLerp(0, rect.width, newPos);
                    sliderVal = Mathf.Lerp(min, max, newSliderVal);
                    GUI.changed = true;
                    Event.current.Use();
                }
                break;
            }
        }
        
    }
    
    public static UIElementS Begin(Rect rect)
    {
        return new UIElementS(rect);
    }

    public UIElementS Draw(Action<Rect> draw)
    {
        var evType = Event.current.type;
        SetState(evType);
        
        if (evType == EventType.Repaint)
        {
            draw(_rect);
        }
        return this;
    }
}

public interface IUIBase
{
    public int ID { get; }
    public Vector2 Position { get; }
    public Vector2 Size { get; }
    public Rect Rect { get; }
    
    public IUIBase? Parent { get; }
    public IReadOnlyCollection<IUIBase> Children { get; }
    
    public UIBase Add(IUIBase element);
}

public struct UIBase : IUIBase
{
    private readonly int _id;
    private readonly Vector2 _pos;
    private readonly Vector2 _size;
    private readonly Rect _rect;
    
    public int ID => _id;
    public Vector2 Position => _pos;
    public Vector2 Size => _size;
    public Rect Rect => _rect;
    
    public IUIBase Parent => throw new NotImplementedException();
    public IReadOnlyCollection<IUIBase> Children => throw new NotImplementedException();

    private UIBase(int id)
    {
        _id = id;
        _pos = Vector2.zero;
        _size = Vector2.zero;
        _rect = Rect.zero;
    }
    
    private UIBase(Rect rect)
    {
        _id = GUIUtility.GetControlID(FocusType.Passive, rect);
        _pos = rect.position;
        _size = rect.size;
        _rect = rect;
    }
    
    public static UIBase Invalid => new UIBase(-1);

    public static UIBase New(Rect rect)
    {
        if (!UIGuards.EnsureEvent(EventType.Layout)) return Invalid;
        return new UIBase(rect);
    }

    public enum Alignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
    
    public static UIBase New(UIBase baseElement, Alignment alignment, Vector2 size)
    {
        if (!UIGuards.EnsureEvent(EventType.Layout)) return Invalid;
        var rectFromBase = baseElement.Rect.GetPart(alignment, size);
        var newEl = new UIBase(rectFromBase);
        return newEl;
    }
    
    public UIBase Add(IUIBase element)
    {
        DGUI_References.Reference(this, element);
        return this;
    }

    public void Draw()
    {
        if (!UIGuards.EnsureEvent(EventType.Repaint)) return;
        DGUI.Raw.DrawBox();
    }
}

public interface IUIElement
{
    public int ID { get; }
}

public interface IUIReferences<T> where T : IUIElement
{
    T RegisterElement(T element);
    T GetElement(int id);
    IReadOnlyCollection<T> GetHeldElements();
}

public partial struct UIElementS : IUIElement
{
    internal int _id;

    public int ID => _id;

    public IUIElement With()
    {
        
    }
}

public struct UIButtonMenu : IUIElement
{
    private UIElementS _base;
    
    public int ID => _base._id;
    
    public UIButtonMenu AddButton(string label, Action clickAction)
    {
        
    }
}

public struct UITopBar : IUIElement, IUIReferences<UIButtonMenu>
{
    private UIElementS _base;
    
    public int ID => _base._id;

    public UIButtonMenu AddMenu(string label)
    {
        
    }
}

public static class DGUI
{
    public static void DrawElement(Rect rect)
    {

    }

    public static void DrawTopBar(Rect rect)
    {
        UIBase menuO1B1 = UIBase.New(rect);
        UIBase menuO1B2 = UIBase.New(rect);
        
        
        UIBase menuO2B1 = UIBase.New(rect);
        UIBase menuO2B2 = UIBase.New(rect);
        
        UIBase menuOption1 = UIBase.New(rect).Add(menuO1B1).Add(menuO1B2);
        UIBase menuOption2 = UIBase.New(rect).Add(menuO2B1).Add(menuO2B2);
        UIBase bar = UIBase.New(rect).Add(menuOption1).Add(menuOption2);

        bar.Draw();
    }

    public static class Raw
    {
        public static UIBase ComboBox<T>(Rect rect, ICollection<T> items, ref T selItem)
        {
            var ui = UIBase.New().OnMouseDown(() =>
            {
                
            });
        }
        
        public static void DrawBox(Rect rect, Color fill, Color border)
        {
            using var ctx = Styles.BoxContext(fill, border);
            DrawBox(rect);
        }
        
        public static void DrawBox(Rect rect, GUIStyle style = null)
        {
            //TODO: Add style context using here
            GUI.Box(rect, GUIContent.none, style);
        }
    }

    public static class Styles
    {
        public static GUIStyleContext BoxContext(Color fill, Color border)
        {
            return new GUIStyleContext(StyleContextType.Box, BoxStyleBGStore.GetBoxStyleBG(fill, border));
        }
    }

    public static class Content
    {
    }

    public static class Colors
    {
        public static readonly Color BackgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    }

    internal static class BoxStyleBGStore
    {
        private static readonly Dictionary<ColorPair, Texture2D> BoxStyleBG = new Dictionary<ColorPair, Texture2D>();

        internal static Texture2D GetBoxStyleBG(Color fill, Color border)
        {
            if (BoxStyleBG.TryGetValue(new ColorPair(fill, border), out Texture2D texture)) return texture;
            texture = GenerateBoxStyleBg(fill, border);
            BoxStyleBG.Add(new ColorPair(fill, border), texture);
            return texture;
        }

        private static  Texture2D GenerateBoxStyleBg(Color fill, Color border)
        {
            Texture2D texture = new Texture2D(3, 3);
            texture.SetPixel(0,0, border);
            texture.SetPixel(1,0, border);
            texture.SetPixel(2,0, border);
            texture.SetPixel(0,1, border);
            texture.SetPixel(1,1, fill);
            texture.SetPixel(2,1, border);
            texture.SetPixel(0,2, border);
            texture.SetPixel(1,2, border);
            texture.SetPixel(2,2, border);
            texture.Apply();
            return texture;
        }
        
        public readonly struct ColorPair(Color color1, Color color2) : IEquatable<ColorPair>
        {
            private readonly Color _color1 = color1;
            private readonly Color _color2 = color2;

            public bool Equals(ColorPair other)
            {
                return _color1.Equals(other._color1) && _color2.Equals(other._color2);
            }

            public override bool Equals(object? obj)
            {
                return obj is ColorPair other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_color1.GetHashCode() * 397) ^ _color2.GetHashCode();
                }
            }
        }

    }
}