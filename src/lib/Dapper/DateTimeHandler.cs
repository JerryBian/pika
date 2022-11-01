using System;

namespace Pika.Lib.Dapper;

public class DateTimeHandler : SqliteTypeHandler<DateTime>
{
    public override DateTime Parse(object value)
    {
        return DateTime.Parse((string)value);
    }
}