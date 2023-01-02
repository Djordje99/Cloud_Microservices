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
        private CloudTable _tableUser;
        private CloudTable _tableUserPurchase;
        private IReliableStateManager _stateManager;
        private CancellationToken _cancellationToken;
        private Thread _userTableThread;
        private Thread _userPurchaseTableThread;
        private IReliableDictionary<string, User> userDictionary;
        private IReliableDictionary<long, UserPurchase> userPurchaseDictionary;

        public UserService(IReliableStateManager stateManager)
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            this._tableUser = tableClient.GetTableReference("User");
            this._tableUser.CreateIfNotExists();

            this._tableUserPurchase = tableClient.GetTableReference("UserPurchase");
            this._tableUserPurchase.CreateIfNotExists();

            this._stateManager = stateManager;
            this._cancellationToken.ThrowIfCancellationRequested();

            this._userTableThread = new Thread(new ThreadStart(UserTableWriteThread));
            this._userPurchaseTableThread = new Thread(new ThreadStart(UserPurchaseTableWriteThread));
        }

        public async Task SetDictionary()
        {
            this.userDictionary = await this._stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("User");
            this.userPurchaseDictionary = await this._stateManager.GetOrAddAsync<IReliableDictionary<long, UserPurchase>>("UserPurchase");

            StartThread();
        }

        public async Task<List<UserPurchase>> GetUserPurchases(string username)
        {
            List<UserPurchase> userPurchases = new List<UserPurchase>();

            using (var tx = this._stateManager.CreateTransaction())
            {
                var user = await this.userDictionary.TryGetValueAsync(tx, username);

                if (user.Value.PurchaseHistory != null)
                {
                    foreach (var userPurchaseId in user.Value.PurchaseHistory)
                    {
                        var userPurchase = await this.userPurchaseDictionary.TryGetValueAsync(tx, userPurchaseId);

                        userPurchases.Add(userPurchase.Value);
                    }
                }
            }

            return userPurchases;
        }

        public async Task SetPurchaseToUser(string username, long purchaseID)
        {
            UserPurchase userPurchase = new UserPurchase()
            { 
                PurchaseId = purchaseID, 
                Username = username,
                ID = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"))
            };

            using(var tx = this._stateManager.CreateTransaction())
            {
                await userPurchaseDictionary.AddAsync(tx, userPurchase.ID, userPurchase);
                await tx.CommitAsync();
            }

            using (var tx = this._stateManager.CreateTransaction())
            {
                var user = await this.userDictionary.TryGetValueAsync(tx, username);

                if (user.Value.PurchaseHistory == null)
                    user.Value.PurchaseHistory = new List<long>() { userPurchase.ID };
                else
                    user.Value.PurchaseHistory.Add(userPurchase.ID);

                await tx.CommitAsync();
            }
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

            using (var tx = this._stateManager.CreateTransaction())
            {
                bool isExists = await this.userDictionary.ContainsKeyAsync(tx, user.Username);

                if (!isExists)
                {
                    await this.userDictionary.AddAsync(tx, user.Username, new User(user));
                    await tx.CommitAsync();
                    status = true;
                }
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

        public async Task CancelPurchase(long userPurchaseId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<UserPurchaseTableEntity>("UserPurchase", userPurchaseId.ToString());
            TableResult result = await this._tableUserPurchase.ExecuteAsync(retrieveOperation);

            UserPurchaseTableEntity entity = result.Result as UserPurchaseTableEntity;

            if (entity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(entity);
                await this._tableUserPurchase.ExecuteAsync(deleteOperation);
            }

            await this.userDictionary.ClearAsync();
            await this.userPurchaseDictionary.ClearAsync();

            LoadUserPurchaseTableData();
            LoadUserTableData();
        }

        #region StartService
        private async void LoadUserTableData()
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                TableQuery<UserTableEntity> query = new TableQuery<UserTableEntity>();

                foreach (UserTableEntity userTable in this._tableUser.ExecuteQuery(query))
                {
                    var user = new User(userTable);

                    var purchases = (await this.userPurchaseDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await purchases.MoveNextAsync(this._cancellationToken))
                    {
                        if (user.Username == purchases.Current.Value.Username)
                            if (user.PurchaseHistory == null)
                                user.PurchaseHistory = new List<long>() { purchases.Current.Value.ID };
                            else
                                user.PurchaseHistory.Add(purchases.Current.Value.ID);
                    }

                    await this.userDictionary.AddAsync(tx, userTable.Username, user);
                }

                await tx.CommitAsync();
            }
        }

        private async void LoadUserPurchaseTableData()
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                TableQuery<UserPurchaseTableEntity> query = new TableQuery<UserPurchaseTableEntity>();

                foreach (UserPurchaseTableEntity userPurchaseTable in this._tableUserPurchase.ExecuteQuery(query))
                {
                    await this.userPurchaseDictionary.AddAsync(tx, userPurchaseTable.ID, new UserPurchase(userPurchaseTable));
                }

                await tx.CommitAsync();
            }
        }

        private void StartThread()
        {
            LoadUserPurchaseTableData();
            LoadUserTableData();
            this._userTableThread.Start();
            this._userPurchaseTableThread.Start();
        }

        private async void UserTableWriteThread()
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

                        await this._tableUser.ExecuteAsync(insertOperation);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        private async void UserPurchaseTableWriteThread()
        {
            while (true)
            {
                using (var tx = this._stateManager.CreateTransaction())
                {
                    var enumerator = (await this.userPurchaseDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(this._cancellationToken))
                    {
                        UserPurchase user = enumerator.Current.Value;

                        TableOperation insertOperation = TableOperation.InsertOrReplace(new UserPurchaseTableEntity(user));

                        await this._tableUserPurchase.ExecuteAsync(insertOperation);
                    }
                }

                Thread.Sleep(5000);
            }
        }
        #endregion
    }
}
