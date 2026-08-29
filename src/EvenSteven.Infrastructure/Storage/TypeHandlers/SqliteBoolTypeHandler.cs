using Dapper;
using System.Data;

namespace EvenSteven.Infrastructure.Storage.TypeHandlers
{
    public class SqliteBoolTypeHandler : SqlMapper.TypeHandler<bool>
    {
        public override bool Parse(object value)
        {
            if (value is null || value is DBNull)
            {
                return false;
            }

            return value.ToString() switch
            {
                "1" or "true" or "True" or "TRUE" => true,
                _ => false,
            };
        }

        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            parameter.Value = value ? 1 : 0;
        }
    }
}
