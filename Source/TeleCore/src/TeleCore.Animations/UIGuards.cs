using UnityEngine;

namespace TeleCore.Animations;

public static class UIGuards
{
    public static bool EnsureEvent(EventType type)
    {
        return Event.current.type == type;
    }
}