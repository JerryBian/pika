using Pika.Lib.Model;

namespace Pika.Web.Models;

public class RunDetailViewModel
{
    public string TaskName { get; set; }

    public PikaTaskRun Run { get; set; }
}