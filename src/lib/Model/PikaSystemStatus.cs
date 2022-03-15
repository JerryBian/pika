using System.Collections.Generic;

namespace Pika.Lib.Model;

public class PikaSystemStatus
{
    public int TaskCount { get; set; }

    public int RunCount { get; set; }

    public int TaskInRunningCount { get; set; }

    public int TaskInPendingCount { get; set; }

    public int TaskInCompletedCount { get; set; }

    public List<PikaTask> MostRunTasks { get; set; } = new();

    public List<PikaTaskRun> LongestRuns { get; set; } = new();

    public long DbSize { get; set; }
}