﻿using TeleCore.Data.Events;
using TeleCore.Events;
using Verse;

namespace TeleCore.Rooms.Updates;

internal struct DelayedCacheAction
{
    public RegionStateChangedArgs Args { get; }
    public IntVec3 Cell { get; }
    public int Index { get; }

    public DelayedCacheAction(RegionStateChangedArgs args, IntVec3 cell, int index)
    {
        Args = args;
        Cell = cell;
        Index = index;
    }
}