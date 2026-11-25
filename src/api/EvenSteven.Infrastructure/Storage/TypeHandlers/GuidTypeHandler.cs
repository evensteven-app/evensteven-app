using System.Data;
using Dapper;

namespace EvenSteven.Infrastructure.Storage.TypeHandlers
{
    internal class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }

        public override Guid Parse(object value)
        {
            return value is Guid guid ? guid : Guid.Parse((string)value);
        }
    }
}
