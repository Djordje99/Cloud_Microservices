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

namespace UserStatefulService.Services
{
    public class UserService : IUserService
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private IReliableStateManager _stateManager;
        private CancellationToken _cancellationToken;
        private Thread _tableThread;
        private IReliableDictionary<string, User> userDictionary;

        public UserService(IReliableStateManager stateManager)
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            this._table = tableClient.GetTableReference("User");
            this._table.CreateIfNotExists();

            this._stateManager = stateManager;
            this._cancellationToken.ThrowIfCancellationRequested();
            this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public async Task SetDictionary()
        {
            this.userDictionary = await this._stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("User"); ;

            StartThread();
        }

        public async Task<bool> LogIn(string username, string password)
        {
            bool status = false;

            using (var tx = this._stateManager.CreateTransaction())
            {
                bool isExists = await this.userDictionary.ContainsKeyAsync(tx, username);

                if (isExists)
                {
                    var user = await this.userDictionary.TryGetValueAsync(tx, username);

                    if (user.Value.Password == password)
                        status = true;
                }

                await tx.CommitAsync();
            }

            return status;
        }

        public async Task<bool> Register(RegisterUser user)
        {
            bool status = false;

            TableOperation retrieveOperation = TableOperation.Retrieve<UserTableEntity>("User", user.Username);
            TableResult result = this._table.Execute(retrieveOperation);

            if (result.Result != null)
                return false;


            using (var tx = this._stateManager.CreateTransaction())
            {
                await this.userDictionary.AddAsync(tx, user.Username, new User(user));
                await tx.CommitAsync();
                status = true;
            }

            return status;
        }

        public async Task<long> GetUsersBankAcount(string username)
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                var user = await userDictionary.TryGetValueAsync(tx, username);

                return user.Value.AccountNumber;
            }
        }

        private async void LoadTableData()
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                TableQuery<UserTableEntity> query = new TableQuery<UserTableEntity>();

                foreach (UserTableEntity userTable in this._table.ExecuteQuery(query))
                {
                    await this.userDictionary.AddAsync(tx, userTable.Username, new User(userTable));
                }

                long currentDictCount = await userDictionary.GetCountAsync(tx);

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
                    var enumerator = (await this.userDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(this._cancellationToken))
                    {
                        User user = enumerator.Current.Value;

                        TableOperation insertOperation = TableOperation.InsertOrReplace(new UserTableEntity(user));

                        await this._table.ExecuteAsync(insertOperation);
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
