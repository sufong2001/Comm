using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureStorage.Interfaces;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using Sufong2001.Share.String;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sufong2001.Comm.BusinessEntities
{
    public static class HelperExtensions
    {
        /// <summary>
        /// return the file name if it matches to CommunicationManifest.FileName
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string IsIfManifest(this string fileName)
        {
            return new[] { fileName }.IsIfManifest();
        }

        /// <summary>
        /// return the first file name if it matches to CommunicationManifest.FileName.
        /// The longer file name will have the highest priority
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static string IsIfManifest(this string[] fileNames)
        {
            return fileNames?.Where(f => f.IsNotNullOrEmpty())
                .OrderByDescending(f => f.Length)
                .FirstOrDefault(f => f.EndsWith(CommunicationManifest.FileName));
        }

        public static string ResolveRowRange(this TimerInfo timer)
        {
            var timestamp = timer.ScheduleStatus.Last;
            var rowRange = timestamp.ToString("yyyyMMdd<");
            return rowRange;
        }

        public static async Task DispatchTo(this IEnumerable<TableEntityAdapter<MessageSchedule>> results, IQueueRepository queueRepository)
        {
            var queue = results.Select(e =>
            {
                var dispatch = e.OriginalEntity.IsOrMap<MessageDispatch>();
                dispatch.RowKey = e.RowKey;

                // Get dispatch queue
                var sendQueue = queueRepository.GetQueue(dispatch.QueueName);
                return dispatch.AddMessageToAsync(sendQueue);
            }).ToArray();

            await Task.WhenAll(queue);
        }
    }
}