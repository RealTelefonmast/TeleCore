using System;
using System.Runtime.InteropServices;

namespace TeleCore.Lib.Utils;

public static class EnumUtils
{
    public static unsafe bool HasFlag<T>(T enumValue, T flag) where T : unmanaged
    {
        return !enumValue.And(flag).IsZero();
    }
}