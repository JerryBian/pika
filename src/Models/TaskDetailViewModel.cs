using Pika.Common.Model;
using System.Collections.Generic;

namespace Pika.Models;

public class TaskDetailViewModel
{
    public PikaTask Task { get; set; }

    public int RunCount { get; set; }

    public List<PikaTaskRun> Runs { get; set; } = [];
}