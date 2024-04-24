using System;
using TeleCore.FlowCore;
using Verse;

namespace TeleCore.Systems.Events;

//TODO: More events, game, map, incidents
public static partial class GlobalEventHandler
{
    public static class Things
    {
        public static event ThingSpawnedEvent? Spawned;
        public static event ThingDespawnedEvent? Despawning;
        public static event ThingDespawnedEvent? Despawned;
        public static event ThingStateChangedEvent? SentSignal;

        public static void RegisterSpawned(ThingSpawnedEvent handler, Predicate<Thing> predicate)
        {
            Spawned += args =>
            {
                if (predicate(args.Thing))
                {
                    handler(args);
                }
            };
        }
        
        public static void RegisterDespawned(ThingDespawnedEvent handle, Predicate<Thing> predicate)
        {
            Despawned += args =>
            {
                if (predicate(args.Thing))
                {
                    handle(args);
                }
            };
        }
        
        internal static void OnSpawned(ThingStateChangedEventArgs args)
        {
            try
            {
                Spawned?.Invoke(args);
                Terrain.OnCellChanged(new CellChangedEventArgs(args));
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register spawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void OnDespawning(ThingStateChangedEventArgs args)
        {
            try
            {
                Despawning?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void OnDespawned(ThingStateChangedEventArgs args)
        {
            try
            {
                Despawned?.Invoke(args);
                Terrain.OnCellChanged(new CellChangedEventArgs(args));
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to deregister despawned thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static void OnSentSignal(ThingStateChangedEventArgs args)
        {
            try
            {
                SentSignal?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to send signal on thing: {args.Thing}\n{ex.Message}\n{ex.StackTrace}");
            }
        }
        
        internal static void Clear()
        {
            Spawned = null;
            Despawning = null;
            Despawned = null;
            SentSignal = null;
        }
    }

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

    public static class Terrain
    {
        public static event TerrainChangedEvent TerrainChanged;
        public static event CellChangedEvent CellChanged;
        
        internal static void OnCellChanged(CellChangedEventArgs args)
        {
            try
            {
                CellChanged?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register cell change: {args.Cell}\n{ex.Message}");
            }
        }
        
        internal static void OnTerrainChanged(TerrainChangedEventArgs args)
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

        public static void Clear()
        {
            TerrainChanged = null;
            CellChanged = null;
        }
    }

    public static class NetworkEvents<T> where T : FlowValueDef
    {
        public static event NetworkVolumeStateChangedEvent<T> NetworkVolumeStateChanged;
        
        internal static void OnVolumeStateChange (FlowVolumeBase<T> flowVolume, VolumeChangedEventArgs<T>.ChangedAction action)
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