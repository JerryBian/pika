using System;

namespace Pika.Lib.Model;

public class PikaTaskRunOutput
{
    public long Id { get; set; }

    public long TaskRunId { get; set; }

    public string Message { get; set; }

    public bool IsError { get; set; }

    public DateTime CreatedAt { get; set; }
}