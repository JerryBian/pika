using System.Collections.Generic;
using Pika.Lib.Model;

namespace Pika.Web.Models;

public class TaskDetailViewModel
{
    public PikaTask Task { get; set; }

    public int RunCount { get; set; }

    public List<PikaTaskRun> Runs { get; set; } = new();
}