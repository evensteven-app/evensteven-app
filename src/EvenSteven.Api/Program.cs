using EvenSteven.Infrastructure.Storage.Migrations;
using FluentMigrator.Runner;
using System.Data.Common;

namespace EvenSteven.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(builder.Configuration.GetConnectionString("db-connection"))
                    .ScanIn(typeof(CreateMainTables).Assembly).For.All()
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

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static void UpdateDatabase(IServiceProvider services)
        {
            var runner = services.GetRequiredService<IMigrationRunner>();

            runner.MigrateUp();
        }
    }
}
