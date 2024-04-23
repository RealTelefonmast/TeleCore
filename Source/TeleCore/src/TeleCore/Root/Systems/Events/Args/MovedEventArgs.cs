﻿using System;
using Verse;

namespace TeleCore.Data.Events;

public class MovedEventArgs : EventArgs
{
    public MovedEventArgs(Thing thing, IntVec3 nextCell, float nextCellCostLeft, float nextCellCostTotal)
    {
        Thing = thing;
        NextCell = nextCell;
        NextCellCostLeft = nextCellCostLeft;
        NextCellCostTotal = nextCellCostTotal;
    }

    public Thing Thing { get; }
    public IntVec3 NextCell { get; }
    public float NextCellCostLeft { get; }
    public float NextCellCostTotal { get; }
}