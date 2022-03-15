using System.Data;
using Dapper;

namespace Pika.Lib.Dapper;

public abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value;
    }
}