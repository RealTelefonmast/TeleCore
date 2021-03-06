using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using Verse;

namespace TeleCore
{
    [StaticConstructorOnStartup]
    internal static class TeleCoreStaticStartup
    {
        static TeleCoreStaticStartup()
        {
            TLog.Message("Startup Init");

            //Manual C# XML-Def Patches
            //TiberiumRimMod.mod.PatchPawnDefs();


            //MP Hook
            TLog.Message($"Multiplayer: {(MP.enabled ? "Enabled - Adding MP hooks..." : "Disabled")}");
            if (MP.enabled)
            {
                MP.RegisterAll();
            }

            //
            TLog.Message("PostLoad Def Changes:");
            ApplyDefChangesPostLoad();
        }

        internal static void ApplyDefChangesPostLoad()
        {
            foreach (var def in DefDatabase<BuildableDef>.AllDefsListForReading)
            {
                DefExtensionCache.TryRegister(def);
            }
        }
    }
}
