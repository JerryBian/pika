using Pika.Common.Model;

namespace Pika.Models;

public class RunDetailViewModel
{
    public string TaskName { get; set; }

    public PikaTaskRun Run { get; set; }
}