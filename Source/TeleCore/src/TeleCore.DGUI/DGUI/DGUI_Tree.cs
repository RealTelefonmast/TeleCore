using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.DGUI;

public struct UIContext
{
    private int _parent;
    private int _id;
    private Rect _rect;

    
    public int ID => _id;
    public int ParentID => _parent;
    public Rect Rect => _rect;
    
    public UIContext(int id, int parentID, Rect rect)
    {
        _id = id;
        _parent = parentID;
        _rect = rect;
    }
}

public static class DGUI_Tree
{
    private static Dictionary<int, UIContext> _contextTree = new();

    public static int CurrentID { get; private set; } = -1;
    public static UIContext CurrentContext { get; private set; }
    
    public static void New(int id, Rect rect)
    {
        var context = new UIContext(id, CurrentID, rect);
        _contextTree.Add(id, context);
        SetCurrent(context);
    }

    public static void SetCurrent(UIContext context)
    {
        CurrentContext = context;
        CurrentID = context.ID;
    }
}

public static class TreeTest
{
    public static void Draw()
    {
        
    }
}