using Microsoft.Azure.Cosmos.Table;
using Sufong2001.Share.Json;
using Sufong2001.Share.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sufong2001.Share.AzureStorage
{
    public static class TableExtensions
    {
        public static async Task<TableResult> CreateIn<T>(this T record, CloudTable cloudTable, string partitionKey,
            string rowKey)
        {
            var entity = record.CreatTableEntity(partitionKey, rowKey);

            var result = await entity.CreateIn(cloudTable);

            return result;
        }

        public static async Task<TableResult> CreateIn(this ITableEntity entity, CloudTable cloudTable)
        {
            var insertOperation = entity.CreateOperationInsert();

            var result = await cloudTable.ExecuteAsync(insertOperation);

            return result;
        }

        public static async Task<TableResult> Update(this ITableEntity entity, CloudTable cloudTable)
        {
            var insertOperation = entity.CreateOperationInsertOrReplace();

            var result = await cloudTable.ExecuteAsync(insertOperation);

            return result;
        }

        public static async Task<IList<TableResult>> CreateIn<T>(this IEnumerable<T> records, CloudTable cloudTable,
            Func<T, string> partitionKey, Func<T, string> rowKey)
        {
            var entities = records
                .Select(e => e.CreatTableEntity(partitionKey(e), rowKey(e)))
                .ToArray();

            var results = await entities.CreateIn(cloudTable);

            return results;
        }

        public static async Task<IList<TableResult>> CreateIn(this IEnumerable<ITableEntity> entities,
            CloudTable cloudTable)
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

        public static async Task<IList<TableResult>> UpdateIn(this IEnumerable<ITableEntity> entities,
            CloudTable cloudTable)
        {
            var batch = new TableBatchOperation();

            ITableEntity BatchUpdate (ITableEntity e)
            {
                batch.InsertOrReplace(e);
                return e;
            }

            var inserts = entities.Select(BatchUpdate).ToArray();

            var results = await cloudTable.ExecuteBatchAsync(batch);

            return results;
        }

        public static async Task<IList<TableResult>> DeleteIn(this IEnumerable<ITableEntity> entities,
            CloudTable cloudTable)
        {
            var batch = new TableBatchOperation();

            ITableEntity BatchDelete(ITableEntity e)
            {
                if (e.ETag.IsNullOrEmpty()) return null;

                batch.Delete(e);
                return e;
            }

            var deleted = entities.Select(BatchDelete)
                .Where(e => e != null)
                .ToArray();

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
            , CloudTable cloudTable, Func<T, string> partitionKey, Func<T, string> rowKey = null,
            Func<T, T> updateOriginalEntity = null)
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

        public static async Task<IEnumerable<TableEntityAdapter<T>>> MoveTo<T>(
            this IEnumerable<TableEntityAdapter<T>> records
            , CloudTable cloudTable, Func<T, string> partitionKey, Func<T, string> rowKey = null,
            Func<T, T> updateOriginalEntity = null)
        {
            var moveEntities = records.Select(record =>
                {
                    var oe = record.OriginalEntity.JClone<T>();

                    return new TableEntityAdapter<T>(
                        updateOriginalEntity == null ? oe : updateOriginalEntity(oe)
                        , partitionKey(oe)
                        , rowKey == null ? record.RowKey : rowKey(oe)
                    );
                })
                .ToArray();

            var operations = new[]
            {
                records.DeleteIn(cloudTable),
                moveEntities.CreateIn(cloudTable),
            };

            await Task.WhenAll(operations);

            return moveEntities;
        }

        public static async Task<TableQuerySegment<TableEntityAdapter<T>>> GetFirstSegmentOf<T>(this CloudTable cloudTable, string partitionKey, string rowKeyEnd)
        {
            var continuationToken = new TableContinuationToken();

            var query = TableHelperExtensions.CreateTopRangeQuery<T>(partitionKey, rowKeyEnd);
            var results = await cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken);

            return results;
        }
    }

    public static class TableHelperExtensions
    {
        /// <summary>
        /// Create the delete TableOperation for the specified TableEntity.
        /// If the ETag IsNullOrEmpty, the TableOperation will not be created and return null
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static TableOperation CreateOperationDelete(this ITableEntity entity)
        {
            return entity.ETag.IsNullOrEmpty() ? null : TableOperation.Delete(entity);
        }

        public static TableOperation CreateOperationInsert(this ITableEntity entity)
        {
            return TableOperation.Insert(entity);
        }

        public static TableOperation CreateOperationInsertOrReplace(this ITableEntity entity)
        {
            return TableOperation.InsertOrReplace(entity);
        }

        public static TableQuery<TableEntityAdapter<T>> CreateTopRangeQuery<T>(string partitionKey, string rowKeyEnd)
        {
            // Create the table query.
            var rangeQuery = new TableQuery<TableEntityAdapter<T>>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, rowKeyEnd)
                    )
                );

            return rangeQuery;
        }

        public static TableEntityAdapter<T> CreatTableEntity<T>(this T record, string partitionKey, string rowKey)
        {
            return new TableEntityAdapter<T>(record, partitionKey, rowKey);
        }

        public static IEnumerable<T> GetResult<T>(this IList<TableResult> results)
        {
            return results.Select(r =>
            {
                if (r.Result is TableEntityAdapter<T> a)
                {
                    return a.OriginalEntity.IsOrMap<T>();
                }

                return r.Result.IsOrMap<T>();
            });
        }
    }
}