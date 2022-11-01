using Pika.Lib.Extension;
using System;

namespace Pika.Lib.Model;

public class PikaTaskRun
{
    public long Id { get; set; }

    public long TaskId { get; set; }

    public string Script { get; set; }

    public long CreatedAt { get; set; }

    public long StartedAt { get; set; }

    public long CompletedAt { get; set; }

    public PikaTaskStatus Status { get; set; }

    public string ShellName { get; set; }

    public string ShellOption { get; set; }

    public string ShellExt { get; set; }

    public DateTime GetCreatedAtTime()
    {
        return new DateTime(CreatedAt);
    }

    public DateTime GetStartedAtTime()
    {
        return new DateTime(StartedAt);
    }

    public DateTime GetCompletedAtTime()
    {
        return new DateTime(CompletedAt);
    }

    public TimeSpan GetTotalElapsed()
    {
        DateTime completedAt = GetCompletedAtTime();
        DateTime end = completedAt == default ? DateTime.Now : completedAt;
        return end - GetCreatedAtTime();
    }

    public TimeSpan GetStartElapsed()
    {
        DateTime startedAt = GetStartedAtTime();
        DateTime end = startedAt == default ? DateTime.Now : startedAt;
        return end - GetCreatedAtTime();
    }

    public TimeSpan GetRunElapsed()
    {
        if (StartedAt == default)
        {
            return TimeSpan.Zero;
        }

        DateTime end = CompletedAt == default ? DateTime.Now : GetCompletedAtTime();
        return end - GetStartedAtTime();
    }

    public string GetStartAtHtml()
    {
        if (StartedAt == default)
        {
            return "<span title=\"Not started yet\">-</span>";
        }

        DateTime startedAt = GetStartedAtTime();
        return $"<span title=\"{startedAt}\">{startedAt.ToHuman()}</span>";
    }

    public string GetCompletedAtHtml()
    {
        if (CompletedAt == default)
        {
            return "<span title=\"Not completed yet\">-</span>";
        }

        DateTime completedAt = GetCompletedAtTime();
        return $"<span title=\"{completedAt}\">{completedAt.ToHuman()}</span>";
    }

    public string GetCreatedAtHtml()
    {
        DateTime createdAt = GetCreatedAtTime();
        return $"<span title=\"{createdAt}\">{createdAt.ToHuman()}</span>";
    }

    public string GetElapsedHtml(bool slim = false)
    {
        TimeSpan totalElapsed = GetTotalElapsed();
        if (slim)
        {
            return $"<span title=\"{totalElapsed}\">{totalElapsed.ToHuman()}</span> ";
        }

        TimeSpan startElapsed = GetStartElapsed();
        TimeSpan runElapsed = GetRunElapsed();
        return
            $"<span title=\"{totalElapsed}\">{totalElapsed.ToHuman()}</span> " +
            $"(<span class=\"fst-italic\" title=\"{startElapsed}\">pending to start</span>: {startElapsed.ToHuman()}, " +
            $"<span class=\"fst-italic\" title=\"{runElapsed}\">run to completed</span>: {runElapsed.ToHuman()})";
    }
}