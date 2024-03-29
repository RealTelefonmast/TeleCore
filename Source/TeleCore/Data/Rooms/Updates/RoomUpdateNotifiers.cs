﻿using System.Collections.Generic;
using TeleCore.Rooms.Updates;
using Verse;

namespace TeleCore;

internal static class RoomUpdateNotifiers
{
    private static RoomTrackerUpdater CurrentWorker { get; set; }

    //Notify roof change on this room group
    public static void Notify_RoofChanged(Room room)
    {
        room.Map.GetMapInfo<MapInformation_Rooms>().Notify_RoofChanged(room);
    }

    //Prepare Room Updates by caching previous room trackers
    public static void Notify_RoomUpdatePrefix(Map map)
    {
        CurrentWorker = map.GetMapInfo<MapInformation_Rooms>().TrackerUpdater;
        CurrentWorker.Notify_UpdateStarted();
    }

    //Set the new room data
    public static void Notify_SetNewRoomData(List<Room> newRoomsData, HashSet<Room> reusedRoomsData)
    {
        CurrentWorker.Notify_NewRooms(newRoomsData, reusedRoomsData);
    }

    //Last step, comparing known data, with new generated rooms
    public static void Notify_RoomUpdatePostfix(Map map)
    {
        CurrentWorker.Notify_Finalize();
        CurrentWorker = null;
    }

    public static void Notify_RoomUpdateSetDirtyCell(IntVec3 cell, Region reg, Map map)
    {
        if (CurrentWorker != null)
        {
            CurrentWorker.Notify_CacheDirtyCell(cell, reg);
        }
        else
        {
            var worker = map.GetMapInfo<MapInformation_Rooms>().TrackerUpdater;
            worker.Notify_CacheDirtyCell(cell, reg);
        }
    }

    public static void Notify_RoomUpdateResetDirtyCell(IntVec3 cell, Map map)
    {
        if (CurrentWorker != null)
        {
            CurrentWorker.Notify_ResetDirtyCell(cell);
        }
        else
        {
            var worker = map.GetMapInfo<MapInformation_Rooms>().TrackerUpdater;
            worker.Notify_ResetDirtyCell(cell);
        }
    }

    public static void Notify_RoomUpdateGetCache(Room room, Map map)
    {
        if (CurrentWorker != null)
        {
            CurrentWorker.Notify_GetDirtyCache(room);
        }
        else
        {
            var worker = map.GetMapInfo<MapInformation_Rooms>().TrackerUpdater;
            worker.Notify_GetDirtyCache(room);
        }
    }
}