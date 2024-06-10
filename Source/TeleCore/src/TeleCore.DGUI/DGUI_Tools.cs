using UnityEngine;

namespace TeleCore.Animations;

public enum ButtonMode
{
    Normal,
    Toggle,
}

public static partial class DGUI
{
    public static bool Button(Rect rect, ButtonMode mode)
    {
        var id = GetId(rect);
        bool result = false;
        switch (mode)
        {
            case ButtonMode.Normal:
                result = GUI.Button(rect, "");
                break;
            case ButtonMode.Toggle:
            {
                var state = GetState(id);
                result = GUI.Toggle(rect, state, "");
                SetState(id);
                break;
            }
        }
        return result;
    }
}