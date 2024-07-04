using System;
using TeleCore.Events.Args;
using TeleCore.FlowCore;
using TeleCore.Loader;
using TeleCore.Shared;
using Verse;

namespace TeleCore.Systems.Events;

//TODO: More events, game, map, incidents
public static partial class GlobalEventHandler
{
    public static class Pawns
    {
        public static event PawnHediffChangedEvent? HediffChanged;
        
        //TODO:
        //public static event PawnEnteredRoomEvent PawnEnteredRoom;
        //public static event PawnLeftRoomEvent PawnLeftRoom;

        internal static void OnPawnHediffChanged(PawnHediffChangedEventArgs args)
        {
            try
            {
                HediffChanged?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register hediff change on pawn: {args.Pawn}\n{ex.Message}");
            }
        }

        internal static void Clear()
        {
            HediffChanged = null;
        }
    }

    public static class Rooms
    {
        public static event RoomCreatedEvent Created;
        public static event RoomDisbandedEvent RoomDisbanded;
        public static event RegionStateEvent CachingRegionStateInfoRoomUpdate;
        public static event RegionStateEvent ResettingRegionStateInfoRoomUpdate;
        public static event RegionStateEvent GettingRegionStateInfoRoomUpdate;

        internal static void OnRoomCreated(RoomChangedArgs args)
        {
            try
            {
                Created?.Invoke(args);
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

        internal static void Clear()
        {
            Created = null;
            RoomDisbanded = null;
            CachingRegionStateInfoRoomUpdate = null;
            ResettingRegionStateInfoRoomUpdate = null;
            GettingRegionStateInfoRoomUpdate = null;
        }
    }

    internal static void ClearData()
    {
        Things.Clear();
        Pawns.Clear();
        Rooms.Clear();
        Terrain.Clear();
    }

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