using EvenSteven.Infrastructure.Storage.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EvenSteven.Infrastructure.Storage.Repositories
{
    public static class AddRepositoryServices
    {
        public static IServiceCollection AddScopedRepositoryServices(this IServiceCollection services)
        {
            services.AddScoped<IRoomRepository, SqliteRoomRepository>();
            services.AddScoped<IParticipantRepository, SqliteParticipantRepository>();
            services.AddScoped<IExpenseRepository, SqliteExpenseRepository>();

            return services;
        }
    }
}
