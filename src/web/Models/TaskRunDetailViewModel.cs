using System;

namespace Pika.Web.Models;

public class TaskRunDetailViewModel
{
    public string TaskName { get; set; }

    public long TaskId { get; set; }

    public long RunId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedAtDisplay { get; set; }

    public string ShellName { get; set; }

    public string ShellOption { get; set; }

    public string ShellExt { get; set; }

    public string Script { get; set; }
}