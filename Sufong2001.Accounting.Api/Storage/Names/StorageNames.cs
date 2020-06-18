namespace Sufong2001.Accounting.Api.Storage.Names
{
    public static class DatabaseConfig
    {
        public const string CosmosDbConnectionString = "CosmosDB:ConnectionString";
    }

    public enum DatabaseName
    {
        Sufong2001
    }

    public enum CommonContainerName
    {
        DataList
    }  
    
    public enum PartitionKeyName
    {
        pk
    }

}