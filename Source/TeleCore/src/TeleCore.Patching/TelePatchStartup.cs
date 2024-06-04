using TeleCore.Loader;

namespace TeleCore.Patching;

[TeleCoreStartupClass]
public class TelePatchStartup
{
    static TelePatchStartup()
    {
        TLog.Message($"TeleCore.Patching...");
        
    }
}