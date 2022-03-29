using System;
using Pika.Lib.Extension;

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

    public string GetStartAtHtml()
    {
        if (StartedAt == default)
        {
            return "<span title=\"Not started yet\">-</span>";
        }

        return $"<span title=\"{StartedAt}\">{StartedAt.ToHuman()}</span>";
    }

    public string GetCompletedAtHtml()
    {
        if (CompletedAt == default)
        {
            return "<span title=\"Not completed yet\">-</span>";
        }

        return $"<span title=\"{CompletedAt}\">{CompletedAt.ToHuman()}</span>";
    }

    public string GetCreatedAtHtml()
    {
        return $"<span title=\"{CreatedAt}\">{CreatedAt.ToHuman()}</span>";
    }

    public string GetElapsedHtml(bool slim = false)
    {
        var totalElapsed = GetTotalElapsed();
        if (slim)
        {
            return $"<span title=\"{totalElapsed}\">{totalElapsed.ToHuman()}</span> ";
        }

        var startElapsed = GetStartElapsed();
        var runElapsed = GetRunElapsed();
        return
            $"<span title=\"{totalElapsed}\">{totalElapsed.ToHuman()}</span> " +
            $"(<span class=\"fst-italic\" title=\"{startElapsed}\">pending to start</span>: {startElapsed.ToHuman()}, " +
            $"<span class=\"fst-italic\" title=\"{runElapsed}\">run to completed</span>: {runElapsed.ToHuman()})";
    }
}