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

namespace TransactionCoordinatorStatefulService.Services
{
    public class TransactionCoordinatorService : ITransactionCordinatorService
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private IReliableStateManager _stateManager;
        private CancellationToken _cancellationToken;
        private Thread _tableThread;
        private long _dictCounter;
        //private IReliableDictionary<string, UserDict> userDict;

        public TransactionCoordinatorService(IReliableStateManager stateManager)
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            this._table = tableClient.GetTableReference("Purchase");
            this._table.CreateIfNotExists();

            this._stateManager = stateManager;
            this._cancellationToken.ThrowIfCancellationRequested();
            this._dictCounter = 0;
            //this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public async Task<bool> BuyDepertureTicket(string username, long departureId, int ticketAmount)
        {
            return true;
        }
    }
}
