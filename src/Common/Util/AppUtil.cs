using System.Diagnostics;
using System.Reflection;

namespace Pika.Common.Util;

public static class AppUtil
{
    public static string AppVersion
    {
        get
        {
            // Try multiple assemblies because single-file publish or native host can change EntryAssembly behavior
            var assembliesToCheck = new Assembly?[]
            {
                Assembly.GetEntryAssembly(),
                Assembly.GetExecutingAssembly(),
                Assembly.GetCallingAssembly(),
                typeof(AppUtil).Assembly
            };

            foreach (var asm in assembliesToCheck)
            {
                var v = GetVersionFromAssembly(asm);
                if (!string.IsNullOrEmpty(v)) return v;
            }

            // As a last resort return 1.0
            return "1.0";
        }
    }

    private static string? GetVersionFromAssembly(Assembly? asm)
    {
        if (asm == null) return null;

        // 1) AssemblyInformationalVersionAttribute
        try
        {
            var infoAttr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (!string.IsNullOrEmpty(infoAttr?.InformationalVersion))
            {
                return infoAttr.InformationalVersion;
            }
        }
        catch { }

        // 2) File/Product version via file location (may be unavailable for single-file apps)
        try
        {
            var location = asm.Location;
            if (!string.IsNullOrEmpty(location))
            {
                var fileVer = FileVersionInfo.GetVersionInfo(location)?.ProductVersion;
                if (!string.IsNullOrEmpty(fileVer))
                {
                    return fileVer;
                }
            }
        }
        catch { }

        // 3) AssemblyName version (Major.Minor)
        try
        {
            var version = asm.GetName().Version;
            if (version != null)
            {
                // If version has non-zero parts return as Major.Minor or Major.Minor.Build
                if (version.Major != 0 || version.Minor != 0 || version.Build != 0)
                {
                    return version.ToString(2);
                }
            }
        }
        catch { }

        return null;
    }
}