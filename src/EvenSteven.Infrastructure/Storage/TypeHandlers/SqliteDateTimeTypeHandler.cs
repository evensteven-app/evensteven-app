using Dapper;
using System.Data;
using System.Globalization;

namespace EvenSteven.Infrastructure.Storage.TypeHandlers
{
    public class SqliteDateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override DateTime Parse(object value)
        {
            return DateTime.Parse((string)value, CultureInfo.InvariantCulture);
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }
    }
}
