using Dapper;
using System.Data;

namespace Pika.Common.Dapper;

public abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value;
    }
}