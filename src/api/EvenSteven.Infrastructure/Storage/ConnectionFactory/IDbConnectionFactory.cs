using System.Data.Common;

namespace EvenSteven.Infrastructure.Storage.ConnectionFactory
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
        Task<DbConnection> CreateOpenedConnectionAsync(CancellationToken ct);
    }
}
