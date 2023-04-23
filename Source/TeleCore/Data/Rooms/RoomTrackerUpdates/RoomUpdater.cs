﻿using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore.Data.Events;
using TeleCore.Data.Logging;
using TeleCore.RoomTrackerUpdates;
using TeleCore.Static.Utilities;
using Verse;

namespace TeleCore;

internal enum DelayedRoomUpdateType
{
    Added,
    Reused,
    Disbanded
}

internal struct DelayedCacheAction
{
    public IntVec3 Cell { get; }
    public int Index { get; }

    public DelayedCacheAction(IntVec3 cell, int index)
    {
        Cell = cell;
        Index = index;
    }
}

internal class DelayedRoomUpdate
{
    public DelayedRoomUpdateType Type { get; }
    public RoomTracker Tracker { get; private set; }
    public RoomTracker[] Previous { get; set; }
    public Room Room { get; }

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

    public override string ToString()
    {
        return $"{Tracker.Room.ID}:{Type}";
    }

    public void SetTracker(RoomTracker newTracker)
    {
        Tracker = newTracker;
    }

    public void SetPrevious(RoomTracker[] previous)
    {
        Previous = previous;
    }
}

public class RoomUpdater
{
    private int lastGameTick = 0;
    private readonly RoomTrackerMapInfo parent;
    
    //
    private readonly List<Room> tempNewRooms = new();
    private readonly List<Room> tempReusedRooms = new();
    
    //
    private readonly List<DelayedRoomUpdate> delayedActions = new ();
    private readonly List<DelayedCacheAction> delayedCacheActions = new ();
    private readonly RoomTracker?[] _trackerGrid;
    
    public bool IsWorking { get; private set; } = false;

    public RoomUpdater(RoomTrackerMapInfo parent)
    {
        this.parent = parent;
        _trackerGrid = new RoomTracker[this.parent.Map.cellIndices.NumGridCells];
    }

    internal void Update(bool isManual = false)
    {
        //Synchronize with game tick
        if ((!IsWorking || Find.TickManager.TicksGame <= lastGameTick) && !isManual) return;

        //
        foreach (var action in delayedActions)
        {
            switch (action.Type)
            {
                case DelayedRoomUpdateType.Added:
                    var previous = action.Room.Cells.Select(c => _trackerGrid[parent.Map.cellIndices.CellToIndex(c)]).Where(t => t != null).Distinct().ToArray();
                    var newTracker = new RoomTracker(action.Room);
                    action.SetTracker(newTracker);
                    action.SetPrevious(previous);
                    parent.SetTracker(newTracker);
                    break;
                case DelayedRoomUpdateType.Disbanded:
                    parent.MarkDisband(action.Tracker);
                    break;
            }
        }
        
        //Reused
        foreach (var action in delayedActions)
        {
            if (action.Type == DelayedRoomUpdateType.Reused)
            {
                action.Tracker.Reset();
                action.Tracker.Notify_Reused();
            }
        }
        
        foreach (var action in delayedActions)
        {
            if (action.Type == DelayedRoomUpdateType.Disbanded) 
                parent.Disband(action.Tracker);
        }
        
        foreach (var action in delayedActions)
        {
            if (action.Type == DelayedRoomUpdateType.Added) 
                action.Tracker.Init(action.Previous);
        }

        foreach (var action in delayedActions)
        {
            if (action.Type == DelayedRoomUpdateType.Added) 
                action.Tracker.PostInit(action.Previous);
        }

        foreach (var action in delayedCacheActions)
        {
            _trackerGrid[action.Index] = null;
        }
        
        //
        IsWorking = false;
        delayedActions.Clear();
    }

    //Cache cells around recently spawned and despawned structures
    internal void Notify_CacheDirtyCell(IntVec3 cell, Region region)
    {
        _trackerGrid[parent.Map.cellIndices.CellToIndex(cell)] = parent[region.Room];
    }
    
    internal void Notify_ResetDirtyCell(IntVec3 cell)
    {
        //_trackerGrid[parent.Map.cellIndices.CellToIndex(cell)] = null;
        delayedCacheActions.Add(new DelayedCacheAction(cell, parent.Map.cellIndices.CellToIndex(cell)));
    }

    internal void Notify_UpdateStarted()
    {
        if (IsWorking)
        {
            for (var i = delayedActions.Count - 1; i >= 0; i--)
            {
                var delayedRoomUpdate = delayedActions[i];
                if (!(delayedRoomUpdate.Room?.Dereferenced ?? true)) continue;
                delayedActions.Remove(delayedRoomUpdate);
            }
            return;
        }
        IsWorking = true;
    }

    internal void Notify_NewRooms(List<Room> newRoomsData, HashSet<Room> reusedRoomsData)
    {
        tempNewRooms.AddRange(newRoomsData);
        tempReusedRooms.AddRange(reusedRoomsData);
    }

    internal void Notify_Finalize()
    {
        //Compare with new generated rooms
        foreach (var newAddedRoom in tempNewRooms)
        {
            delayedActions.Add(new DelayedRoomUpdate(DelayedRoomUpdateType.Added, newAddedRoom));
        }

        foreach (var tracker in parent.AllTrackers.Values)
        {
            if (tempReusedRooms.Contains(tracker.Room))
            {
                delayedActions.Add(new DelayedRoomUpdate(DelayedRoomUpdateType.Reused, tracker));
            }

            if (tracker.Room.Dereferenced)
            {
                delayedActions.Add(new DelayedRoomUpdate(DelayedRoomUpdateType.Disbanded, tracker));
            }
        }

        //
        lastGameTick = Find.TickManager.TicksGame;
        tempNewRooms.Clear();
        tempReusedRooms.Clear();

        //During map generation, update immediately
        if (Current.ProgramState != ProgramState.Playing)
        {
            Update(true);
        }
    }
}