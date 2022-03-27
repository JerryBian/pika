using System;
using Humanizer;
using Humanizer.Localisation;

namespace Pika.Lib.Model;

public class PikaTaskRun
{
    public long Id { get; set; }

    public long TaskId { get; set; }

    public string Script { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime CompletedAt { get; set; }

    public PikaTaskStatus Status { get; set; }

    public string ShellName { get; set; }

    public string ShellOption { get; set; }

    public string ShellExt { get; set; }

    public TimeSpan GetTotalElapsed()
    {
        var end = CompletedAt == default ? DateTime.Now : CompletedAt;
        return end - CreatedAt;
    }

    public TimeSpan GetStartElapsed()
    {
        var end = StartedAt == default ? DateTime.Now : StartedAt;
        return end - CreatedAt;
    }

    public TimeSpan GetRunElapsed()
    {
        if (StartedAt == default)
        {
            return TimeSpan.Zero;
        }

        var end = CompletedAt == default ? DateTime.Now : CompletedAt;
        return end - StartedAt;
    }

    public string GetElapsedHtml()
    {
        var totalElapsed = GetTotalElapsed();
        var startElapsed = GetStartElapsed();
        var runElapsed = GetRunElapsed();
        return
            $"<span title=\"{totalElapsed}\">{totalElapsed.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour)}</span> " +
            $"(<span class=\"fst-italic\" title=\"{startElapsed}\">pending to start</span>: {startElapsed.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour)}, " +
            $"<span class=\"fst-italic\" title=\"{runElapsed}\">run to completed</span>: {runElapsed.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour)})";
    }
}