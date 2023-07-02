﻿using System;
using Verse;

namespace TeleCore.Rooms.Updates;

internal class DelayedRoomUpdate : IDisposable
{
    public DelayedRoomUpdate(DelayedRoomUpdateType type, Room room)
    {
        Type = type;
        Room = room;
    }

    public DelayedRoomUpdate(DelayedRoomUpdateType type, RoomTracker tracker)
    {
        Type = type;
        Tracker = tracker;
    }

    public DelayedRoomUpdateType Type { get; }
    public RoomTracker Tracker { get; private set; }
    public RoomTracker?[] Previous { get; set; }
    public Room Room { get; private set; }

    public void Dispose()
    {
        Tracker = null;
        Previous = null;
        Room = null;
    }

    public override string ToString()
    {
        return $"{Tracker.Room.ID}:{Type}";
    }

    public void SetTracker(RoomTracker newTracker)
    {
        Tracker = newTracker;
    }

    public void SetPrevious(RoomTracker?[] previous)
    {
        Previous = previous;
    }
}