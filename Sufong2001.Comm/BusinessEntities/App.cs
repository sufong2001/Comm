using Sufong2001.Comm.AzureStorage;
using System;

namespace Sufong2001.Comm.BusinessEntities
{
    public class App
    {
        public CommRepository Repository { get; set; }

        public App(CommRepository repository)
        {
            Repository = repository;
            repository.CreateStorageIfNotExists().ConfigureAwait(false);
        }

        /// <summary>
        /// Return Now
        /// </summary>
        public DateTime DateTimeNow => DateTime.Now;
    }
}