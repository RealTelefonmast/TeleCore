﻿using Verse.AI;

namespace TeleCore;

public struct PawnPathResult
{
    public PawnPath Path { get; }

    public string Reason { get; }

    public PawnPathResult(PawnPath result, string reason = null)
    {
        Path = result;
        Reason = reason;
    }
}