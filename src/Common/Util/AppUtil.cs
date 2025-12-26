using System.Reflection;

namespace Pika.Common.Util;

public static class AppUtil
{
    public static string AppVersion
    {
        get
        {
            var asm = typeof(AppUtil).Assembly;
            var infoAttr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (!string.IsNullOrEmpty(infoAttr?.InformationalVersion))
            {
                return infoAttr.InformationalVersion;
            }

            var version = asm.GetName().Version;
            if (version != null && (version.Major != 0 || version.Minor != 0))
            {
                return $"{version.Major}.{version.Minor}";
            }

            return "1.0";
        }
    }
}