using EvenSteven.Infrastructure.Storage.ConnectionFactory;
using EvenSteven.Infrastructure.Storage.Migrations;
using EvenSteven.Infrastructure.Storage.TypeHandlers;
using FluentMigrator.Runner;

namespace EvenSteven.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
            builder.Services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(builder.Configuration.GetConnectionString("db-connection"))
                    .ScanIn(typeof(MigrationAssemblyMarker).Assembly).For.All()
                )
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // Applying migrations
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
            TypeHandlersManager.RegisterSqliteTypeHandlers();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
