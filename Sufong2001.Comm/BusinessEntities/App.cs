using Sufong2001.Comm.AzureStorage.Interfaces;
using System;

namespace Sufong2001.Comm.BusinessEntities
{
    public class App
    {
        public ICommRepository Repository { get; set; }

        public App(ICommRepository repository)
        {
            Repository = repository;
        }

        /// <summary>
        /// Return Now
        /// </summary>
        public DateTime DateTimeNow => DateTime.Now;
    }
}