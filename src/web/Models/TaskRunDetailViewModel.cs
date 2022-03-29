using Pika.Lib.Model;

namespace Pika.Web.Models;

public class TaskRunDetailViewModel
{
    public PikaTask Task { get; set; }

    public PikaTaskRun Run { get; set; }
}