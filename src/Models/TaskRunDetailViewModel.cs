using Pika.Common.Model;

namespace Pika.Models;

public class TaskRunDetailViewModel
{
    public PikaTask Task { get; set; }

    public PikaTaskRun Run { get; set; }
}