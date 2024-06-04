using System;
using JetBrains.Annotations;
using Prepatcher;
using Mono.Cecil;
using Verse;

namespace TeleCore.Patching;

/// <summary>
/// I blame Bradson (use his mods they are awesome)
/// Inspired by his Prepatch Wrapper
/// https://github.com/bbradson/Performance-Fish/blob/main/Source/PerformanceFish/Prepatching/FreePatchEntry.cs
/// </summary>
public static class PrepatchEntry
{
    [FreePatch]
    [UsedImplicitly]
    public static void Start(ModuleDefinition module)
    {
        Log.Message("Assembly References: ");
        foreach (var assRef in AppDomain.CurrentDomain.GetAssemblies())
        {
            Log.Message(" - " + assRef.FullName);
        }
        
        try
        {
            //var myType = typeof(TLog);
            //Log.Message("Tlog: " + myType?.AssemblyQualifiedName);
            var types = typeof(TelePatch).AllSubclassesNonAbstract();
            if (types.Count > 0)
            {
                foreach (var type in types)
                {
                    Log.Message("Patch: " + type.FullName);
                }
            }
        }
        catch (Exception e)
        {
            Log.Message("Prepatcher HATES ME: " + e);
        }
    }
}