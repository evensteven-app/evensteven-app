
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
            // TODO: Change to retrieving connectionString from .env
            string connectionString = "";
            var builder = WebApplication.CreateBuilder(args);

            // Register type handlers for correct db work
            builder.Services.RegisterTypeHandlers();

            builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();

            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(MigrationAssemblyMarker).Assembly).For.All())
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

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
