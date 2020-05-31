using Microsoft.Azure.Cosmos.Table;

namespace Sufong2001.Comm.AzureStorage.Interfaces
{
    public interface ITableRepository
    {
        CloudTable GetTable(string tableName);
    }
}