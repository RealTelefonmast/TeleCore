﻿using System;
using Verse;

namespace TeleCore.Data.Events;

public struct ThingStateChangedEventArgs
{
    public ThingChangeFlag ChangeMode { get; }
    public Thing Thing { get; }
    public string? CompSignal { get; }
    
    public ThingStateChangedEventArgs(ThingChangeFlag changeMode, Thing thing, string compSignal = null)
    {
        ChangeMode = changeMode;
        Thing = thing;
        CompSignal = compSignal;
    }
}