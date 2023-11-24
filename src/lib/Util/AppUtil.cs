using System.Reflection;

namespace Pika.Lib.Util;

public static class AppUtil
{
    public static string AppVersion
    {
        get
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            return version == null ? "1.0" : version.ToString(2);
        }
    }
}