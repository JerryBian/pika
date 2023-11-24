using Pika.Lib.Model;
using System.Collections.Generic;

namespace Pika.Web.Models;

public class TaskDetailViewModel
{
    public PikaTask Task { get; set; }

    public int RunCount { get; set; }

    public List<PikaTaskRun> Runs { get; set; } = [];
}