using System;
using UnityEngine;
using Verse;

namespace TeleCore.DGUI.ECS;

public readonly struct UIEntity(int id)
{
    public int ID { get; } = id;
}

public interface IComponent{}

public struct RectComponent(Rect rect) : IComponent
{
    public Rect Value = rect;
}

public struct TextComponent(string text) : IComponent
{
    public string Value = text;
}

public struct BoxComponent(Color fill, Color border) : IComponent
{
    public Color Fill = fill;
    public Color Border = border;
}

public struct OnClickComponent(Action action) : IComponent
{
    public Action Value = action;
}

public static class UIECS
{
    
    
    public static void Test()
    {
        //Test: Draw a box, with a label and a button.
        
        //Fluent API?
        //var area = New().WithBox(Color.white, Color.black);
        //area = area.WithArea(Slice.Pct(Side.Left, 0.5f).Contract(5,5,5,5), (e) => e.WithLabel("Button:"));
        //area = area.WithArea(Slice.Pct(Side.Right, 0.5f).Contract(5,5,5,5), (e) => e.WithButton("Click Me!", () => Console.WriteLine("Clicked the button!")));
        
        //ECS?
        var box = Archetypes.Box(new Rect(), Color.white, Color.black);
        var rect = box.Component<RectComponent>();
        var label = Archetypes.Label(rect.Rect.LeftPart(0.5f), "Button:");
        var button = Archetypes.Button(rect.Rect.RightPart(0.5f), "Click Me!", () => Console.WriteLine("Clicked the button!"));
    }

    public static class Archetypes
    {
        public static UIEntity Box(Rect rect, Color fill, Color border)
        {
            var ent = New(rect);
            ent.Attach(new BoxComponent(fill, border));

            return ent;
        }
        
        public static UIEntity Button(Rect rect, Action clickAction)
        {
            var ent = New(rect);
            ent.Attach(new OnClickComponent(clickAction));
            return ent;
        }
    }

    public static UIEntity New(Rect rect)
    {
        var id = GUIUtility.GetControlID(FocusType.Passive, rect);
        var ent = new UIEntity(id);
        ent.Attach(new RectComponent(rect));
        return ent;
    }

    public static UIEntity Attach<T>(this UIEntity ent, T comp) where T : IComponent
    {
        
        return ent;
    }
}