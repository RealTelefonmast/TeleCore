using System;
using System.Runtime.CompilerServices;
using Verse;

namespace TeleCore.Loader;

[StaticConstructorOnStartup]
public static class TeleCoreStaticStartup
{
    public static event Action? OnStartup;

    static TeleCoreStaticStartup()
    {
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
        
                
        TLog.Message("TeleCore: Startup");
        
        //DefID Validator
        TeleStartUtils.DefIDValidation();
        TeleStartUtils.ExecuteDefInjectors();
        
        OnStartup?.Invoke();   
    }
    
}