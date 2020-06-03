using Microsoft.Azure.Cosmos.Table;

namespace Sufong2001.Core.Storage.Interfaces
{
    public interface ITableRepository
    {
        CloudTable GetTable(string tableName);
    }
}