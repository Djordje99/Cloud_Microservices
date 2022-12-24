using Common.DTO;
using Common.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
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
        private UserTableHelper _table;
        private CancellationToken _cancellationToken;
        private Thread _tableThread;
        private long _dictCounter;

        public UserService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
            this._table = new UserTableHelper();
            this._cancellationToken.ThrowIfCancellationRequested();
            this._dictCounter = 0;
            this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public Task LogIn(string username, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Register(UserDict user)
        {
            var userDict =  await this._stateManager.GetOrAddAsync<IReliableDictionary<string, UserDict>>("User");
            bool status = false;

            using(var tx = this._stateManager.CreateTransaction())
            {
                await userDict.AddAsync(tx, user.Username, user);
                await tx.CommitAsync();
                status = true;
            }

            StartThread();

            return status;
        }

        private void StartThread()
        {
            if (this._dictCounter == 0)
                this._tableThread.Start();
        }

        private async void TableWriteThread()
        {
            var userDict = await this._stateManager.GetOrAddAsync<IReliableDictionary<string, UserDict>>("User");

            while (true)
            {
                using (var tx = this._stateManager.CreateTransaction())
                {
                    long currentDictCount = await userDict.GetCountAsync(tx);

                    if (this._dictCounter != currentDictCount)
                    {
                        var enumerator = (await userDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                        while (await enumerator.MoveNextAsync(this._cancellationToken))
                        {
                            UserDict user = enumerator.Current.Value;

                            await this._table.AddUser(new User(user));
                        }

                        this._dictCounter = currentDictCount;
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
