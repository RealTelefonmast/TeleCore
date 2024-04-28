using System.Collections.Generic;
using TeleCore.Systems.Events;
using Verse;

namespace TeleCore.Static;

//TODO: Is this needed?
public static class QuickAccess
{
    private static Dictionary<Thing, RoomTracker> _roomTrackerByThing;

    static QuickAccess()
    {
        _roomTrackerByThing = new();

        GlobalEventHandler.Things.Spawned += HandleThingSpawn;
        GlobalEventHandler.Things.Despawned += HandleThingDespawn;

        GlobalEventHandler.Rooms.Created += HandleCreated;
    }

    private static void HandleCreated(RoomChangedArgs args)
    {
        args.RoomTracker.Room.Regions[0].ListerThings.AllThings.ForEach(thing =>
        {
            if (_roomTrackerByThing.ContainsKey(thing))
            {
                _roomTrackerByThing[thing] = args.RoomTracker;
            }
            else
            {
                _roomTrackerByThing.Add(thing, args.RoomTracker);
            }
        });
    }

    private static void HandleThingDespawn(ThingStateChangedEventArgs args)
    {

    }

    private static void HandleThingSpawn(ThingStateChangedEventArgs args)
    {

    }
}