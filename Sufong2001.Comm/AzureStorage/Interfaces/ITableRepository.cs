using Microsoft.WindowsAzure.Storage.Table;

namespace Sufong2001.Comm.AzureStorage.Interfaces
{
    public interface ITableRepository
    {
        CloudTable GetTable(string tableName);
    }
}