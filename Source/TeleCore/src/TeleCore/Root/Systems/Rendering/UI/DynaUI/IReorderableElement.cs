﻿using UnityEngine;

namespace TeleCore;

public interface IReorderableElement
{
    public UIElement Element { get; }
    public void DrawElementInScroller(Rect inRect);
}