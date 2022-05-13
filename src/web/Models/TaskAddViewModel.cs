namespace Pika.Web.Models;

public class TaskAddViewModel
{
    public bool IsTemp { get; set; }

    public string TaskName { get; set; }

    public string TaskDescription { get; set; }

    public string ShellName { get; set; }

    public string ShellOption { get; set; }

    public string ShellExt { get; set; }

    public string Script { get; set; }
}