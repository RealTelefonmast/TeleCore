﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TeleCore;
using TeleCore.Data.Events;
using TeleCore.Defs;
using TeleCore.FlowCore;
using TeleCore.Static;
using UnityEngine.Profiling;
using Verse;

namespace TeleCore
{
    public static class StaticData
    {
        //
        public const float DeltaTime = 1f / 60f;

        //
        internal static Dictionary<SubBuildMenuDef, SubBuildMenu> windowsByDef;

        internal static Dictionary<int, MapComponent_TeleCore> teleMapComps;
        internal static Dictionary<BuildableDef, Designator> cachedDesignators;
        
        //
        internal static List<PlaySettingsWorker> _playSettings;

        //Static Props
        public static WorldComp_TeleCore TeleCoreWorldComp { get; internal set; }

        public static MapComponent_TeleCore TeleMapComp(int mapInt) => teleMapComps[mapInt];

        internal static List<PlaySettingsWorker> PlaySettings => _playSettings;

        static StaticData()
        {
            Notify_ClearData();
            SetupPlaySettings();
        }

        internal static void ExposeStaticData()
        {
            Scribe_Collections.Look(ref windowsByDef, "windowsByDef", LookMode.Def, LookMode.Deep);
        }
        
        internal static void Notify_ClearData()
        {
            //TLog.Message("Clearing StaticData!");
            teleMapComps = new Dictionary<int, MapComponent_TeleCore>();
            cachedDesignators = new Dictionary<BuildableDef, Designator>();
            windowsByDef = new Dictionary<SubBuildMenuDef, SubBuildMenu>();
            ActionComposition._ID = 0;
            
            ClipBoardUtility.Notify_ClearData();
        }

        private static void SetupPlaySettings()
        {
            _playSettings = new List<PlaySettingsWorker>();
            foreach (var type in typeof(PlaySettingsWorker).AllSubclassesNonAbstract())
            {
                _playSettings.Add((PlaySettingsWorker)Activator.CreateInstance(type));
            }
        }
        
        internal static void Notify_ClearingMapAndWorld()
        {
            TFind.TickManager.ClearGameTickers();
            GlobalEventHandler.ClearData();
        }

        internal static void Notify_NewTeleMapComp(MapComponent_TeleCore mapComp)
        {
            teleMapComps[mapComp.map.uniqueID] = mapComp;
        }

        internal static void Notify_NewTeleWorldComp(WorldComp_TeleCore worldComp)
        {
            TeleCoreWorldComp ??= worldComp;
        }

        #region Def ID
        
        /// <summary>
        /// Returns the relative Def object assignable to the provided ID.
        /// </summary>
        /// <param name="id">The ID of the Def.</param>
        /// <typeparam name="TDef">The Def type to search through.</typeparam>
        /// <returns>A unique Def instance as identified by the id.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDef ToDef<TDef>(this ushort id)
            where TDef:Def
        {
            return DefIDStack.ToDef<TDef>(id);
            if (id > DefDatabase<TDef>.AllDefsListForReading.Count)
            {
                TLog.Warning($"Trying to access ID: {id} of {typeof(TDef)} with database of {DefDatabase<TDef>.AllDefsListForReading.Count} | {DefDatabase<FlowValueDef>.AllDefsListForReading.Count}");
                return null;
            }
            return DefDatabase<TDef>.AllDefsListForReading[id]; //DefIDStack<TDef>.GetDef(id);
        }

        /// <summary>
        /// Turns a Def to its relative ID.
        /// <para>Can be used to handle Defs in a more lightweight way without assigning references to objects.</para>
        /// </summary>
        /// <param name="def">The Def instance.</param>
        /// <typeparam name="TDef">The Def type to assign the ID from.</typeparam>
        /// <returns>A unique ID for the Def instance of the given Def type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ToID<TDef>(this TDef def)
            where TDef : Def
        {
            return DefIDStack.ToID(def);// def.index; //DefIDStack<TDef>.GetID(def);
        }

        #endregion

        //
        public static MapComponent_TeleCore TeleCore(this Map map)
        {
            if (map != null) return teleMapComps[map.uniqueID];
            
            TLog.Warning("Map is null for TeleCore MapComp getter");
            return null;
        }
        
        public static ThingGroupCacheInfo ThingGroupCache(this Map map)
        {
            return map.TeleCore().ThingGroupCacheInfo;
        }

        public static WorldComp_TeleCore WorldCompTele()
        {
            return Find.World.GetComponent<WorldComp_TeleCore>();
        }
    }
}
