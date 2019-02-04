using Sufong2001.Comm.AzureStorage.Names;

namespace Sufong2001.Comm.Models.Events
{
    public class MessageDispatch
    {
        public string RowKey { get; set; }

        public string Type { get; set; }

        public string QueueName => $"{QueueNames.CommSend}-{Type.ToLower()}";
    }
}