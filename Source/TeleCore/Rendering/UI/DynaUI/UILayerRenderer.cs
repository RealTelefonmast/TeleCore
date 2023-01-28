﻿using System;
using UnityEngine;
using Verse;

namespace TeleCore;

public static class UILayerRenderer
{
    //Todo: Layered rendering of additional popups within DynaUI
    public static void BeginLayeredView()
    {
        
    }

    public static void DrawLayeredViews()
    {
        
    }

    //
    public static void DrawImmediateLayer(int layer, Rect rect, Action renderAction)
    {
        Widgets.BeginGroup(rect);
        {
            Widgets.DrawMenuSection(rect);
            var leftRect = rect.LeftPartPixels(300).ContractedBy(5).Rounded();
            var rightRect = rect.RightPartPixels(200).Rounded();
        }
        Widgets.EndGroup();
    }
}