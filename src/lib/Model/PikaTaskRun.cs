using System;

namespace Pika.Lib.Model;

public class PikaTaskRun
{
    public long Id { get; set; }

    public long TaskId { get; set; }

    public string Script { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime CompletedAt { get; set; }

    public PikaTaskStatus Status { get; set; }

    public TimeSpan GetElapsed()
    {
        var end = CompletedAt == default ? DateTime.Now : CompletedAt;
        return end - CreatedAt;
    }
}