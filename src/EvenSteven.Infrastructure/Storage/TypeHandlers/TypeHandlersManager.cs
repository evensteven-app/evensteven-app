using Dapper;

namespace EvenSteven.Infrastructure.Storage.TypeHandlers
{
    public class TypeHandlersManager
    {
        public static void RegisterSqliteTypeHandlers()
        {
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.RemoveTypeMap(typeof(bool));

            SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
            SqlMapper.AddTypeHandler(new SqliteDateTimeTypeHandler());
            SqlMapper.AddTypeHandler(new SqliteBoolTypeHandler());
        }
    }
}
