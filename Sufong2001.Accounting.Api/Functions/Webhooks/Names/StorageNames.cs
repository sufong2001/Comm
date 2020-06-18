using System.ComponentModel;

namespace Sufong2001.Accounting.Api.Functions.Webhooks.Names
{

    public static class WebhookQueueNames
    {
        public const string ProcessWebhookEvents = "webhook-events";
    }

    public enum WebhookContainerName
    {
        WebhookPayloads
    }

    public enum PartitionKeyValue
    {
        New,
        Processed
    }
}