using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Share.Json;
using Sufong2001.Share.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureStorage
{
    public static class TableExtensions
    {
        public static async Task CreateIn<T>(this T record, CloudTable cloudTable, string partitionKey, string rowKey)
        {
            var entity = new TableEntityAdapter<T>(record, partitionKey, rowKey);

            await entity.CreateIn(cloudTable);
        }

        public static async Task CreateIn(this ITableEntity entity, CloudTable cloudTable)
        {
            var insertOperation = TableOperation.Insert(entity);

            await cloudTable.ExecuteAsync(insertOperation);
        }

        public static async Task Update(this ITableEntity entity, CloudTable cloudTable)
        {
            var insertOperation = TableOperation.InsertOrReplace(entity);

            await cloudTable.ExecuteAsync(insertOperation);
        }

        public static async Task CreateIn<T>(this IEnumerable<T> records, CloudTable cloudTable, Func<T, string> partitionKey, Func<T, string> rowKey)
        {
            var entities = records
                .Select(e => new TableEntityAdapter<T>(e, partitionKey(e), rowKey(e)))
                .ToArray();

            await entities.CreateIn(cloudTable);
        }

        public static async Task<IList<TableResult>> CreateIn(this IEnumerable<ITableEntity> entities, CloudTable cloudTable)
        {
            var batch = new TableBatchOperation();

            ITableEntity BatchInsert(ITableEntity e)
            {
                batch.Insert(e);
                return e;
            }

            var inserts = entities.Select(BatchInsert).ToArray();

            var results = await cloudTable.ExecuteBatchAsync(batch);

            return results;
        }

        public static T CloneTo<T>(this T entity, string partitionKey) where T : ITableEntity
        {
            var clone = entity.JClone<T>();

            clone.PartitionKey = partitionKey;

            return clone;
        }

        public static async Task<TableEntityAdapter<T>> MoveTo<T>(this TableEntityAdapter<T> record
            , CloudTable cloudTable, Func<T, string> partitionKey, Func<T, string> rowKey = null, Func<T, T> updateOriginalEntity = null)
        {
            // update the OriginalEntity.Property value has cause some funny error without cloning the object
            // System.Private.CoreLib: Exception while executing function: TransferEnd.
            // Microsoft.Azure.WebJobs.Host: Error while handling parameter upload after function returned:.
            // Microsoft.WindowsAzure.Storage: Not Found.
            // var oe = record.OriginalEntity;
            var oe = record.OriginalEntity.JClone<T>();

            var moveEntity = new TableEntityAdapter<T>(
                updateOriginalEntity == null ? oe : updateOriginalEntity(oe)
                , partitionKey(oe)
                , rowKey == null ? record.RowKey : rowKey(oe)
            );

            var operations = new[]
                {
                    record.CreateOperationDelete(),
                    moveEntity.CreateOperationInsert(),
                }
                .Where(op => op != null)
                .Select(cloudTable.ExecuteAsync)
                .ToArray();

            await Task.WhenAll(operations);

            return moveEntity;
        }

        /// <summary>
        /// Create the delete TableOperation for the specified TableEntity.
        /// If the ETag IsNullOrEmpty, the TableOperation will not be created and return null
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static TableOperation CreateOperationDelete(this TableEntity entity)
        {
            return entity.ETag.IsNullOrEmpty() ? null : TableOperation.Delete(entity);
        }

        public static TableOperation CreateOperationInsert(this TableEntity entity)
        {
            return TableOperation.Insert(entity);
        }
    }
}