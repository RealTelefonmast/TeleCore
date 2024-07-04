using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TeleCore.Static;
using Verse;

namespace TeleCore;

public static class StaticData
{
    public const float DeltaTime = 1f / 60f;

    internal static Dictionary<BuildableDef, Designator> DESIGNATORS;

    //Build Menu
    internal static Dictionary<SubBuildMenuDef, SubBuildMenu> BUILDMENU_BY_DEF;

    //Tele Data Comps
    internal static Dictionary<int, MapComponent_TeleCore> TELE_MAPCOMPS;
    internal static Dictionary<string, WorldComp_TeleCore> TELE_WORLDCOMPS;

    //
    internal static List<PlaySettingsWorker> _playSettings;

    static StaticData()
    {
        Notify_ClearData();
        SetupPlaySettings();
    }

    //Static Props
    internal static List<PlaySettingsWorker> PlaySettings => _playSettings;

    public static MapComponent_TeleCore TeleMapComp(int mapInt)
    {
        return TELE_MAPCOMPS[mapInt];
    }

    public static WorldComp_TeleCore TeleWorldComp(string worldID)
    {
        return TELE_WORLDCOMPS[worldID];
    }

    internal static void ExposeStaticData()
    {
        Scribe_Collections.Look(ref BUILDMENU_BY_DEF, "windowsByDef", LookMode.Def, LookMode.Deep);
    }

    internal static void Notify_ClearData()
    {
        TELE_MAPCOMPS = new Dictionary<int, MapComponent_TeleCore>();
        TELE_WORLDCOMPS = new Dictionary<string, WorldComp_TeleCore>();

        DESIGNATORS = new Dictionary<BuildableDef, Designator>();
        BUILDMENU_BY_DEF = new Dictionary<SubBuildMenuDef, SubBuildMenu>();
        ActionComposition._ID = 0;

        ClipBoardUtility.Notify_ClearData();
    }

    private static void SetupPlaySettings()
    {
        _playSettings = new List<PlaySettingsWorker>();
        foreach (var type in typeof(PlaySettingsWorker).AllSubclassesNonAbstract())
            _playSettings.Add((PlaySettingsWorker)Activator.CreateInstance(type));
    }

    internal static void Notify_NewTeleMapComp(MapComponent_TeleCore mapComp)
    {
        TELE_MAPCOMPS[mapComp.map.uniqueID] = mapComp;
    }

    internal static void Notify_NewTeleWorldComp(WorldComp_TeleCore worldComp)
    {
        TELE_WORLDCOMPS[worldComp.world.GetUniqueLoadID()] = worldComp;
    }

    //
    public static MapComponent_TeleCore TeleCore(this Map? map)
    {
        if (map != null)
            return TELE_MAPCOMPS[map.uniqueID];

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