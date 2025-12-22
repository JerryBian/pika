using System.Collections.Generic;

namespace Pika.Common.Model;

public class PikaSystemStatus
{
    public int RunCount { get; set; }

    public List<PikaTask> SavedTasks { get; set; } = [];

    public List<PikaTaskRun> LatestRuns { get; set; } = [];

    public long DbSize { get; set; }
}