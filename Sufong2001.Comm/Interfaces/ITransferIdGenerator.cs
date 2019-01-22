namespace Sufong2001.Comm.Interfaces
{
    public interface ITransferIdGenerator
    {
        string TransferSessionId();
    }

    public interface IMessageIdGenerator
    {
        string MessageId();
    }

}