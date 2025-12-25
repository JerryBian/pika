using Pika.Common.Extension;

namespace Pika.Common.Model
{
    public class PikaDriveSmartctlTable
    {
        public string DriveId { get; set; }

        public bool Passed { get; set; }

        public long? ReallocatedSectorCt { get; set; }

        public long? CurrentPendingSector { get; set; }

        public long? OfflineUncorrectable { get; set; }

        public long? ReportedUncorrect { get; set; }

        public long? StartStopCount { get; set; }

        public long? PowerOnHours { get; set; }

        public long CreatedAt { get; set; }

        public string CreatedAtString => new DateTime(CreatedAt).ToHuman();

        public long? PowerCycleCount { get; set; }

        public long? CommandTimeout { get; set; }

        public long? PowerOffRestartCount { get; set; }

        public long? LoadCycleCount { get; set; }

        public long? UdmaCrcErrorCount { get; set; }

        public long? Temperature { get; set; }
    }
}
