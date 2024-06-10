using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Verse;

namespace TeleCore.Loader;

[StaticConstructorOnStartup]
public static class TeleCoreStaticStartup
{
    public static event Action? OnStartup;

    internal static Stopwatch StopWatch { get; private set; }
    
    static TeleCoreStaticStartup()
    {
        StopWatch = new Stopwatch();
        TLog.Message("TeleCore: Startup");
        
        //Init Startup types
        foreach (var type in GenTypes.AllTypesWithAttribute<TeleCoreStartupClassAttribute>())
        {
            try
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
            catch (Exception e)
            {
                TLog.Error($"Failed to run startup constructor for {type}: {e.Message}");
            }
        }
        
        TLog.Message($"TeleCore: Loaded Modules {StopWatch.ElapsedMilliseconds}ms");
        
        //DefID Validator
        TeleStartUtils.DefIDValidation();
        TeleStartUtils.ExecuteDefInjectors();
        
        OnStartup?.Invoke();   
    }
    
}