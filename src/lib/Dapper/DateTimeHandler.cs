using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pika.Lib.Dapper
{
    public class DateTimeHandler : SqliteTypeHandler<DateTime>
    {
        public override DateTime Parse(object value)
        {
            return DateTime.Parse((string) value);
        }
    }
}
