using Common.DTO;
using Common.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserStatefulService.TableHelper;

namespace UserStatefulService.Services
{
    public class UserService : IUserService
    {
        private IReliableStateManager _stateManager;
        private UserTableHelper _tableHelper;
        private CancellationToken _cancellationToken;
        private Thread _tableThread;
        private long _dictCounter;
        private IReliableDictionary<string, UserDict> userDict;

        public UserService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
            this._tableHelper = new UserTableHelper("UserTable");
            this._cancellationToken.ThrowIfCancellationRequested();
            this._dictCounter = 0;
            this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public async Task<bool> LogIn(string username, string password)
        {
            StartThread();

            bool status = false;

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                bool isExists = await this.userDict.ContainsKeyAsync(tx, username);

                if (isExists)
                {
                    var user = await this.userDict.TryGetValueAsync(tx, username);

                    if (user.Value.Password == password)
                        status = true;
                }

                await tx.CommitAsync(); 
            }

            return status;
        }

        public async Task<bool> Register(UserDict user)
        {
            StartThread();

            bool status = false;

            TableOperation retrieveOperation = TableOperation.Retrieve<User>("User", user.Username);
            TableResult result = this._tableHelper.Table.Execute(retrieveOperation);

            if (result.Result != null)
                return false;


            using (var tx = this._stateManager.CreateTransaction())
            {
                await this.userDict.AddAsync(tx, user.Username, user);
                await tx.CommitAsync();
                status = true;
            }

            return status;
        }

        private async void LoadTableData()
        {
            this.userDict = await this._stateManager.GetOrAddAsync<IReliableDictionary<string, UserDict>>("User");

            using (ITransaction tx = this._stateManager.CreateTransaction())
            {
                TableQuery<User> query = new TableQuery<User>();

                foreach (User user in this._tableHelper.Table.ExecuteQuery(query))
                {
                    await this.userDict.AddAsync(tx, user.Username, new UserDict(user));
                }

                long currentDictCount = await userDict.GetCountAsync(tx);

                await tx.CommitAsync();

                this._dictCounter = currentDictCount;
            }
        }

        private void StartThread()
        {
            if (this._dictCounter == 0)
            {
                LoadTableData();
                this._tableThread.Start();
            }
        }

        private async void TableWriteThread()
        {
            while (true)
            {
                using (var tx = this._stateManager.CreateTransaction())
                {
                    long currentDictCount = await this.userDict.GetCountAsync(tx);

                    if (this._dictCounter != currentDictCount)
                    {
                        var enumerator = (await this.userDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                        while (await enumerator.MoveNextAsync(this._cancellationToken))
                        {
                            UserDict user = enumerator.Current.Value;

                            TableOperation insertOperation = TableOperation.InsertOrReplace(new User(user));

                            await this._tableHelper.Table.ExecuteAsync(insertOperation);
                        }

                        this._dictCounter = currentDictCount;
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
