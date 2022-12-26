using Common.DTO;
using Common.Interfaces;
using Microsoft.Azure;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DepartureStatefulService.Services
{
    public class DepartureService : IDepartureService
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private IReliableStateManager _stateManager;
        private System.Threading.CancellationToken _cancellationToken;
        private Thread _tableThread;
        private long _dictCounter;
        private IReliableDictionary<string, UserDict> departureDict;

        public DepartureService(IReliableStateManager stateManager)
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            this._table = tableClient.GetTableReference("Departure");
            this._table.CreateIfNotExists();

            this._stateManager = stateManager;
            this._cancellationToken.ThrowIfCancellationRequested();
            this._dictCounter = 0;
            //this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public async Task<bool> CreateDeparture(Departure departure)
        {
            TableOperation retrieveOperation = TableOperation.InsertOrReplace(new DepartureTableEntity(departure));
            await this._table.ExecuteAsync(retrieveOperation);

            return true;
        }

        public async Task<List<Departure>> ListDeparture()
        {
            List<Departure> departureList = new List<Departure>();

            TableQuery<DepartureTableEntity> query = new TableQuery<DepartureTableEntity>();

            foreach (DepartureTableEntity entity in this._table.ExecuteQuery(query))
            {
                //only dates less than now
                departureList.Add(new Departure(entity));
            }

            return departureList;
        }
    }
}
