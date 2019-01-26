namespace Sufong2001.Comm.Interfaces
{
    public interface IUploadIdGenerator
    {
        string UploadSessionId();
    }

    public interface IMessageIdGenerator
    {
        string MessageId();
    }

}