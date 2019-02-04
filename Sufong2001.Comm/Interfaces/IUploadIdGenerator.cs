using System;

namespace Sufong2001.Comm.Interfaces
{
    public interface IUploadIdGenerator
    {
        string UploadSessionId();
    }

    public interface IMessageIdGenerator
    {
        string MessageId(string sessionId);
    }

    public interface IScheduleIdGenerator
    {
        string ScheduleId(DateTime dateTime);
    }
}