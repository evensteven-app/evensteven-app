using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace EvenSteven.Infrastructure.Storage.TypeHandlers
{
    public static class TypeHandlersManager
    {
        public static IServiceCollection RegisterTypeHandlers(this IServiceCollection services)
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new DateTimeTypeHandler());

            return services;
        }
    }
}
