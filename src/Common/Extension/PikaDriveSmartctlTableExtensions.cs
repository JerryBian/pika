using Pika.Common.Model;
using Pika.Common.Util;

namespace Pika.Common.Extension
{
    public static class PikaDriveSmartctlTableExtensions
    {
        public static string LabelsJson(this IEnumerable<PikaDriveSmartctlTable> tables)
        {
            if (tables == null) return "[]";
            var labels = tables.OrderBy(s => s.CreatedAt).Select(s => s.CreatedAtString);
            return JsonUtil.Serialize(labels);
        }

        private static string DataJson<T>(this IEnumerable<PikaDriveSmartctlTable> tables, Func<PikaDriveSmartctlTable, T> selector)
        {
            if (tables == null) return "[]";
            var data = tables.OrderBy(s => s.CreatedAt).Select(selector);
            return JsonUtil.Serialize(data);
        }

        public static string ReallocatedSectorCtDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.ReallocatedSectorCt);

        public static string CurrentPendingSectorDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.CurrentPendingSector);

        public static string OfflineUncorrectableDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.OfflineUncorrectable);

        public static string ReportedUncorrectDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.ReportedUncorrect);

        public static string StartStopCountDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.StartStopCount);

        public static string PowerOnHoursDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.PowerOnHours);

        public static string PowerCycleCountDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.PowerCycleCount);

        public static string CommandTimeoutDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.CommandTimeout);

        public static string PowerOffRestartCountDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.PowerOffRestartCount);

        public static string LoadCycleCountDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.LoadCycleCount);

        public static string UdmaCrcErrorCountDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.UdmaCrcErrorCount);

        public static string TemperatureDataJson(this IEnumerable<PikaDriveSmartctlTable> tables) =>
            tables.DataJson(s => s.Temperature);
    }
}
