using System.ComponentModel;

namespace Sufong2001.Accounting.Api.Functions.Webhooks.Names
{
    public enum DatabaseName
    {
        Sufong2001
    }

    public static class WebhookQueueNames
    {
        public const string WebhookEvents = "webhook-events";
    }

    public enum WebhookContainerName
    {
        WebhookPayloads
    }

    public enum PartitionKeyName
    {
        pk
    }

    public enum PartitionKeyValue
    {
        New,
        Processed
    }
}