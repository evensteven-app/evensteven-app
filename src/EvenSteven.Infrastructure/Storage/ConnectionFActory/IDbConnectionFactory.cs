using System.Data;

namespace EvenSteven.Infrastructure.Storage.ConnectionFActory
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
