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

namespace BankStatefulService.Services
{
    public class BankService : IBankService
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private IReliableStateManager _stateManager;
        private CancellationToken _cancellationToken;
        private Thread _tableThread;
        private ITransaction _transaction;
        private long _localAccountNumber;
        private double _localPrice;
        private IReliableDictionary<long, BankAccount> _bankDictionary;

        public BankService(IReliableStateManager stateManager)
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            this._table = tableClient.GetTableReference("Bank");
            this._table.CreateIfNotExists();

            this._stateManager = stateManager;
            this._cancellationToken.ThrowIfCancellationRequested();

            this._transaction = null;
            this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public async Task SetDictionary()
        {
            this._bankDictionary = await this._stateManager.GetOrAddAsync<IReliableDictionary<long, BankAccount>>("Bank"); ;

            StartThread();
        }

        public async Task<bool> CreateBankAccount(BankAccount account)
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                await this._bankDictionary.AddAsync(tx, account.AccountNumber, account);
                await tx.CommitAsync();
            }

            return true;
        }

        public async Task<bool> EnlistMoneyTransfer(long accountNumber, double price)
        {
            this._localPrice = price;
            this._localAccountNumber = accountNumber;

            var isPrepared = await Prepare();

            return isPrepared;
        }

        public async Task<bool> Prepare()
        {
            this._transaction = this._stateManager.CreateTransaction();

            var account = await _bankDictionary.TryGetValueAsync(this._transaction, this._localAccountNumber);

            if (account.Value.AvailableAssets >= this._localPrice)
            {
                account.Value.AvailableAssets = account.Value.AvailableAssets - this._localPrice;
                await _bankDictionary.AddOrUpdateAsync(this._transaction, this._localAccountNumber, account.Value, (key, value) => value);
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

        private async void LoadTableData()
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                TableQuery<BankAccountTableEntity> query = new TableQuery<BankAccountTableEntity>();

                foreach (BankAccountTableEntity account in this._table.ExecuteQuery(query))
                {
                    await this._bankDictionary.AddAsync(tx, account.AccountNumber, new BankAccount(account));
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
                    var enumerator = (await this._bankDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(this._cancellationToken))
                    {
                        BankAccount account = enumerator.Current.Value;

                        TableOperation insertOperation = TableOperation.InsertOrReplace(new BankAccountTableEntity(account));

                        await this._table.ExecuteAsync(insertOperation);
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
