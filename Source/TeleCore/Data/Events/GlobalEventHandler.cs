﻿using System;
using TeleCore.Events;
using TeleCore.FlowCore;

namespace TeleCore.Data.Events;

public static class GlobalEventHandler
{
    #region Things

    public static event ThingSpawnedEvent ThingSpawned;
    public static event ThingDespawnedEvent ThingDespawning;
    public static event ThingDespawnedEvent ThingDespawned;
    public static event ThingStateChangedEvent ThingSentSignal;

    #endregion

    
    #region Pawns

    public static event PawnHediffChangedEvent PawnHediffChanged;
    //TODO:
    //public static event PawnEnteredRoomEvent PawnEnteredRoom;
    //public static event PawnLeftRoomEvent PawnLeftRoom;

    #endregion
    
    
    #region Network

    public static class NetworkEvents<T> where T : FlowValueDef
    {
        public static event NetworkVolumeStateChangedEvent<T> NetworkVolumeStateChanged;
        
        public static void OnVolumeStateChange (FlowVolumeBase<T> flowVolume, VolumeChangedEventArgs<T>.ChangedAction action)
        {
            try
            {
                NetworkVolumeStateChanged?.Invoke(new VolumeChangedEventArgs<T>(action, flowVolume));
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register volume change: {flowVolume}\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    #endregion

    
    #region Rooms/Regions

    public static event RoomCreatedEvent RoomCreated;
    public static event RoomDisbandedEvent RoomDisbanded;

    // public static event RegionStateEvent CachingRegionStateInfo;
    // public static event RegionStateEvent ResettingRegionStateInfo;
    // public static event RegionStateEvent GettingRegionStateInfo;
    
     public static event RegionStateEvent CachingRegionStateInfoRoomUpdate;
    public static event RegionStateEvent ResettingRegionStateInfoRoomUpdate;
    public static event RegionStateEvent GettingRegionStateInfoRoomUpdate;
    
    #endregion
    
    public static event TerrainChangedEvent TerrainChanged;
    public static event CellChangedEvent CellChanged;
    

    #region Terrain

    public static void OnTerrainChanged(TerrainChangedEventArgs args)
    {
        try
        {
            TerrainChanged?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to register terrain change: {args.PreviousTerrain} -> {args.NewTerrain}\n{ex.Message}");
        }
    }

    #endregion

    //
    internal static void OnPawnHediffChanged(PawnHediffChangedEventArgs args)
    {
        try
        {
            PawnHediffChanged?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to register hediff change on pawn: {args.Pawn}\n{ex.Message}");
        }
    }

    internal static void ClearData()
    {
        // Things
        ThingSpawned = null;
        ThingDespawning = null;
        ThingDespawned = null;
        ThingSentSignal = null;

        // Pawns
        PawnHediffChanged = null;

        // Network
        //TODO:

        // Rooms
        RoomCreated = null;
        RoomDisbanded = null;

        // Cell and Terrain 
        TerrainChanged = null;
        CellChanged = null;
    }


    #region Things

    internal static void OnThingSpawned(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingSpawned?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to register spawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingDespawning(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingDespawning?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingDespawned(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingDespawned?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    internal static void OnThingSentSignal(ThingStateChangedEventArgs args)
    {
        try
        {
            ThingSentSignal?.Invoke(args);
            CellChanged?.Invoke(new CellChangedEventArgs(args));
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to send signal on thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region Rooms

    internal static void OnRoomCreated(RoomChangedArgs args)
    {
        try
        {
            RoomCreated?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to register room creation: {args.RoomTracker.Room.ID}\n{ex.Message}\n{ex.StackTrace}");
        }
    }
    
    internal static void OnRoomDisbanded(RoomChangedArgs args)
    {
        try
        {
            RoomDisbanded?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify disbanded room:\n{ex}");
        }
    }
    
    internal static void OnRoomReused(RoomChangedArgs args)
    {
        try
        {
            RoomDisbanded?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify reused room:\n{ex}");
        }
    }
    
    /*internal static void OnRegionStateCached(RegionStateChangedArgs args)
    {
        try
        {
            CachingRegionStateInfo?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify cached region state:\n{ex}");
        }
    }
    
    internal static void OnRegionStateReset(RegionStateChangedArgs args)
    {
        try
        {
            ResettingRegionStateInfo?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify reset region state:\n{ex}");
        }
    }
    
    internal static void OnRegionStateGet(RegionStateChangedArgs args)
    {
        try
        {
            GettingRegionStateInfo?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify get region state:\n{ex}");
        }
    }*/
    internal static void OnRegionStateCachedRoomUpdate(RegionStateChangedArgs action)
    {
        try
        {
            CachingRegionStateInfoRoomUpdate?.Invoke(action);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify reset region state:\n{ex}");
        }
    }

    internal static void OnRegionStateResetRoomUpdate(RegionStateChangedArgs action)
    {
        try
        {
            ResettingRegionStateInfoRoomUpdate?.Invoke(action);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify reset region state:\n{ex}");
        }
    }
    
    internal static void OnRegionStateGetRoomUpdate(RegionStateChangedArgs action)
    {
        try
        {
            GettingRegionStateInfoRoomUpdate?.Invoke(action);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify cached region state:\n{ex}");
        }
    }
    
    #endregion

    #region Verbs

    public static event ProjectileLaunchedEvent ProjectileLaunched;
    
    internal static void OnProjectileLaunched(ProjectileLaunchedArgs args)
    {
        try
        {
            ProjectileLaunched?.Invoke(args);
        }
        catch (Exception ex)
        {
            TLog.Error($"Error trying to notify projectile launch: {args}\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion
}