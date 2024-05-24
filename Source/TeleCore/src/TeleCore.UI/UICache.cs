using System.Collections.Generic;

namespace TeleCore.UI;

internal static class UICache
{
    private static readonly Dictionary<string, string> _stringBuffers = new Dictionary<string, string>();

    public static string GetBuffer(string ctrlName)
    {
        _stringBuffers.TryAdd(ctrlName, "");
        return _stringBuffers[ctrlName];
    }
}