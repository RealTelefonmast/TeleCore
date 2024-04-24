using System;
using Verse;

namespace TeleCore.Systems.Events;

[StaticConstructorOnStartup]
public class StaticEventHandler
{
    internal static Action ApplicationQuitEvent;
    internal static Action ClearingMapAndWorld;
    
    static StaticEventHandler()
    {
        ClearingMapAndWorld += GlobalEventHandler.ClearData;
        ClearingMapAndWorld += GlobalUpdateEventHandler.ClearData;
    }
    
    internal static void CheckEventHandler<T>(ref T eventColl, bool clear = false) where T : Delegate
    {
        if (eventColl.GetInvocationList().Length > 0)
        {
            TLog.Warning($"Event {eventColl.Method.Name} has not been unsubscribed from properly.");
            if (clear)
            {
                eventColl = null!;
            }
        }
    }

    public static void OnClearingMapAndWorld()
    {
        try
        {
            ClearingMapAndWorld?.Invoke();
        }
        catch (Exception ex)
        {
            TLog.Error($"Error while trying to clear map and world data.\n{ex.Message}");
        }
        
    }
}