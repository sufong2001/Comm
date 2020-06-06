using Microsoft.Azure.Cosmos;

namespace Sufong2001.Core.Storage.Interfaces
{
    public interface ICosmosDbRepository
    {
        Container GetContainer(string containerName);

    }
}