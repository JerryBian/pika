using System.Reflection;

namespace Pika.Lib.Util;

public static class AppUtil
{
    public static string AppVersion
    {
        get
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            if (version == null)
            {
                return "1.0.0";
            }

            return version.ToString(3);
        }
    }
}