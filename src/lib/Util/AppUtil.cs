using System.Reflection;

namespace Pika.Lib.Util;

public static class AppUtil
{
    public static string AppVersion
    {
        get
        {
            System.Version version = Assembly.GetEntryAssembly()?.GetName().Version;
            return version == null ? "1.0.0" : version.ToString(3);
        }
    }
}