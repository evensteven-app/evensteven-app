using Dapper;
using System.Data;

namespace EvenSteven.Infrastructure.Storage.TypeHandlers
{
    public class SqliteInt64TypeHandler : SqlMapper.TypeHandler<long>
    {
        public override long Parse(object value)
        {
            if (value is DBNull)
            {
                return 0;
            }

            return Convert.ToInt64(value);
        }

        public override void SetValue(IDbDataParameter parameter, long value)
        {
            parameter.Value = value;
        }
    }
}