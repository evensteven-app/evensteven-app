using Dapper;
using System.Data;
using System.Globalization;

namespace EvenSteven.Infrastructure.Storage.TypeHandlers
{
    internal class DateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value.ToString("O");
        }

        public override DateTime Parse(object value)
        {
            return value is DateTime date ? date : DateTime.Parse((string)value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}
