
using EvenSteven.Infrastructure.Storage.ConnectionFactory;
using EvenSteven.Infrastructure.Storage.TypeHandlers;

namespace EvenSteven.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register type handlers for correct db work
            builder.Services.RegisterTypeHandlers();

            builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();

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
