﻿using System.Collections.Generic;

namespace TeleCore;

/// <summary>
/// A room's neighbor can be defined in different ways:
/// <para>- A true neighbor (ie: Doorways)</para>
/// <para>- Any attached room (Rooms on the other side of doors/walls)</para>
/// So we need to track of that.
/// </summary>
public class RoomNeighborSet
{
    private List<RoomTracker> _trueNghb;
    private List<RoomTracker> _attachedNghb;
    
    public IReadOnlyCollection<RoomTracker> TrueNeighbors => _trueNghb;
    public IReadOnlyCollection<RoomTracker> AttachedNeighbors => _attachedNghb;

    public RoomNeighborSet()
    {
        _trueNghb = new List<RoomTracker>();
        _attachedNghb = new List<RoomTracker>();
    }
    
    public void Notify_AddNeighbor(RoomTracker neighbor)
    {
        _trueNghb.Add(neighbor);
    }
    
    public void Notify_AddAttachedNeighbor(RoomTracker neighbor)
    {
        _attachedNghb.Add(neighbor);
    }

    public void Reset()
    {
        _trueNghb.Clear();
        _attachedNghb.Clear();
    }
}