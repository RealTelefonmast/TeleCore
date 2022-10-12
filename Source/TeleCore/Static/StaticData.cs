﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public static class StaticData
    {
        //
        public const float DeltaTime = 1f / 60f;

        //
        internal static Dictionary<SubBuildMenuDef, Window> windowsByDef;
        internal static Dictionary<int, MapComponent_TeleCore> teleMapComps;
        internal static Dictionary<ThingDef, Designator> cachedDesignators;

        public static MapComponent_TeleCore TeleMapComp(int mapInt) => teleMapComps[mapInt];

        static StaticData()
        {
            Notify_Reload();
        }

        internal static void Notify_Reload()
        {
            TLog.Message("Clearing StaticData!");
            teleMapComps = new Dictionary<int, MapComponent_TeleCore>();
            cachedDesignators = new Dictionary<ThingDef, Designator>();
            windowsByDef = new Dictionary<SubBuildMenuDef, Window>();
            ActionComposition._ID = 0;
        }

        internal static void Notify_ClearingMapAndWorld()
        {
            TFind.TickManager.ClearGameTickers();
        }

        internal static void Notify_NewTibMapComp(MapComponent_TeleCore mapComp)
        {
            teleMapComps[mapComp.map.uniqueID] = mapComp;
        }

        public static MapComponent_TeleCore TeleCore(this Map map)
        {
            if (map == null)
            {
                TLog.Warning("Map is null for TeleCore MapComp getter");
                return null;
            }
            return teleMapComps[map.uniqueID];
        }

        public static WorldComp_Tele WorldCompTele()
        {
            return Find.World.GetComponent<WorldComp_Tele>();
        }

    }
}
