using Dapper;
using EvenSteven.Infrastructure.Storage.Migrations;
using EvenSteven.Infrastructure.Storage.TypeHandlers;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace EvenSteven.RepositoryTests
{
    public class TestDatabase(string dbName, string connectionString) : IAsyncDisposable
    {
        public string DbName = dbName;
        public string ConnectionString = connectionString;

        public static TestDatabase Create()
        {
            TypeHandlersManager.RegisterSqliteTypeHandlers();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "../../../"));
            string folderPath = Path.Combine(projectRoot, "Test Databases");

            Directory.CreateDirectory(folderPath);

            string databaseFileName = $"test_{Guid.NewGuid():N}.sqlite";
            string fullDatabasePath = Path.Combine(folderPath, databaseFileName);

            var testConnectionString = new SqliteConnectionStringBuilder { DataSource = fullDatabasePath }.ConnectionString;
            RunMigration(testConnectionString);

            return new TestDatabase(fullDatabasePath, testConnectionString);
        }

        public async Task ResetAsync()
        {
            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync("PRAGMA foreign_keys = OFF;");

            var tables = await connection.QueryAsync<string>(
                "SELECT name FROM sqlite_master WHERE type = 'table' AND name != 'sqlite_sequence';");

            foreach (var table in tables)
            {
                await connection.ExecuteAsync($"DELETE FROM {table};");

                if (tables.Contains("sqlite_sequence"))
                {
                    await connection.ExecuteAsync(
                        "DELETE FROM sqlite_sequence WHERE name = @Table;",
                        new { Table = table });
                }
            }

            await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
        }

        public ValueTask DisposeAsync()
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(DbName))
            {
                try
                {
                    File.Delete(DbName);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private static void RunMigration(string connectionString)
        {
            var serviceProvider = CreateServices(connectionString);

            using var scope = serviceProvider.CreateScope();
            UpdateDatabase(scope.ServiceProvider);
        }

        private static ServiceProvider CreateServices(string connectionString)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(MigrationAssemblyMarker).Assembly).For.Migrations())
                .BuildServiceProvider(false);
        }

        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            runner.MigrateUp();
        }
    }
}
