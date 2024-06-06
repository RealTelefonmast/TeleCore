using UnityEngine;

namespace TeleCore.Animations;

public static class UIUtils
{
    public static Rect GetPart(this Rect rect, UIBase.Alignment alignment, Vector2 size)
    {
        switch (alignment)
        {
            case UIBase.Alignment.TopLeft:
                return new Rect(rect.x, rect.y, size.x, size.y);
            case UIBase.Alignment.TopCenter:
                return new Rect(rect.x + (rect.width - size.x) / 2, rect.y, size.x, size.y);
            case UIBase.Alignment.TopRight:
                
    }
}