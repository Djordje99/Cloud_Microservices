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
        private ITransaction _transaction;
        private IReliableDictionary<long, Departure> _departureDictionary;
        private long _localId;
        private int _localTickeAmount;

        public DepartureService(IReliableStateManager stateManager)
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            this._table = tableClient.GetTableReference("Departure");
            this._table.CreateIfNotExists();

            this._stateManager = stateManager;
            this._cancellationToken.ThrowIfCancellationRequested();
            this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public async Task SetDictionary()
        {
            this._departureDictionary = await this._stateManager.GetOrAddAsync<IReliableDictionary<long, Departure>>("Departure"); ;

            StartThread();
        }

        public async Task<double> GetPrice()
        {
            double price = 0;

            try
            {
                var departure = await this._departureDictionary.TryGetValueAsync(this._transaction, this._localId);

                price = departure.Value.Price * this._localTickeAmount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return price;
        }

        public async Task<bool> EnlistTicketPurchase(long departureId, int ticketAmount)
        {
            this._localId = departureId;
            this._localTickeAmount = ticketAmount;

            var isPurchased = await Prepare();

            return isPurchased; 
        }

        public async Task<bool> Prepare()
        {
            this._transaction = this._stateManager.CreateTransaction();

            var departure = await this._departureDictionary.TryGetValueAsync(this._transaction, this._localId);

            if (departure.Value.DepartureAvaiableTicketCount >= this._localTickeAmount)
            {
                departure.Value.DepartureAvaiableTicketCount = departure.Value.DepartureAvaiableTicketCount - this._localTickeAmount;
                await _departureDictionary.AddOrUpdateAsync(this._transaction, this._localId, departure.Value, (key, value) => value);
                return true;
            }

            return false;
        }

        public async Task Commit()
        {
            await this._transaction.CommitAsync();
            this._transaction.Dispose();
            this._transaction = null;
        }

        public async Task Rollback()
        {
            this._transaction.Abort();
            this._transaction.Dispose();
            this._transaction = null;
        }

        public async Task<bool> CreateDeparture(Departure departure)
        {
            departure.ID =  Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));

            using (var tx = this._stateManager.CreateTransaction())
            {
                await this._departureDictionary.AddAsync(tx, departure.ID, departure);
                await tx.CommitAsync();
            }

            //TableOperation retrieveOperation = TableOperation.InsertOrReplace(new DepartureTableEntity(departure));
            //await this._table.ExecuteAsync(retrieveOperation);

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

        private async void LoadTableData()
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                TableQuery<DepartureTableEntity> query = new TableQuery<DepartureTableEntity>();

                foreach (DepartureTableEntity departure in this._table.ExecuteQuery(query))
                {
                    await this._departureDictionary.AddAsync(tx, departure.ID, new Departure(departure));
                }

                await tx.CommitAsync();
            }
        }

        private void StartThread()
        {
            LoadTableData();
            this._tableThread.Start();
        }

        private async void TableWriteThread()
        {
            while (true)
            {
                using (var tx = this._stateManager.CreateTransaction())
                {
                    var enumerator = (await this._departureDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(this._cancellationToken))
                    {
                        Departure departure = enumerator.Current.Value;

                        TableOperation insertOperation = TableOperation.InsertOrReplace(new DepartureTableEntity(departure));

                        await this._table.ExecuteAsync(insertOperation);
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
