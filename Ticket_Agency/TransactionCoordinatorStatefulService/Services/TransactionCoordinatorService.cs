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
using System.ServiceModel;
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
        private IReliableDictionary<long, Purchase> purchaseDictionary;

        public TransactionCoordinatorService(IReliableStateManager stateManager)
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            this._table = tableClient.GetTableReference("Purchase");
            this._table.CreateIfNotExists();

            this._stateManager = stateManager;
            this._cancellationToken.ThrowIfCancellationRequested();
            this._dictCounter = 0;
            this._tableThread = new Thread(new ThreadStart(TableWriteThread));
        }

        public async Task SetDictionary()
        {
            this.purchaseDictionary = await this._stateManager.GetOrAddAsync<IReliableDictionary<long, Purchase>>("Purchase"); ;

            StartThread();
        }

        public async Task CancelPurchase(long purchaseId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<PurchaseTableEntity>("Purchase", purchaseId.ToString());
            TableResult result = await this._table.ExecuteAsync(retrieveOperation);

            PurchaseTableEntity entity = result.Result as PurchaseTableEntity;

            if (entity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(entity);
                await this._table.ExecuteAsync(deleteOperation);
            }

            await this.purchaseDictionary.ClearAsync();

            LoadTableData();
        }

        public async Task<bool> BuyDepertureTicket(string username, long departureId, int ticketAmount)
        {
            bool isBought = false;

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddressBank = new EndpointAddress("net.tcp://localhost:20025/BankService");
            var endpointAddressDeparture = new EndpointAddress("net.tcp://localhost:20015/DepartureService");
            var endpointAddressUser = new EndpointAddress("net.tcp://localhost:20001/UserService");

            using (var channelFactoryBank = new ChannelFactory<IBankService>(binding, endpointAddressBank))
            {
                IBankService bank = null;
                using (var channelFactoryDeparture = new ChannelFactory<IDepartureService>(binding, endpointAddressDeparture))
                {
                    IDepartureService departure = null;
                    using(var channelFactoryUser = new ChannelFactory<IUserService>(binding, endpointAddressUser))
                    {
                        IUserService user = null;
                        try
                        {
                            departure = channelFactoryDeparture.CreateChannel();
                            bank = channelFactoryBank.CreateChannel();
                            user = channelFactoryUser.CreateChannel();

                            var accountNumber =  await user.GetUsersBankAcount(username);

                            var departureResult = await departure.EnlistTicketPurchase(departureId, ticketAmount);

                            var price = await departure.GetPrice();

                            var bankResult = await bank.EnlistMoneyTransfer(accountNumber, price);

                            if (departureResult && bankResult)
                            {
                                var purchaseID = await SavePurchaseToDictionary(departureId, ticketAmount);

                                await user.SetPurchaseToUser(username, purchaseID);

                                isBought = true;

                                await departure.Commit();
                                await bank.Commit();
                            }
                            else
                            {
                                await departure.Rollback();
                                await bank.Rollback();
                                isBought = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            await departure.Rollback();
                            await bank.Rollback();
                            isBought = false;
                        }
                    }
                }
            }

            return isBought;
        }

        private async Task<long> SavePurchaseToDictionary(long departureId, int ticketAmount)
        {
            var purchase = new Purchase()
            {
                DepartureID = departureId,
                ID = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff")),
                TicketPurchaseCount = ticketAmount
            };

            using (var tx = this._stateManager.CreateTransaction())
            {
                await this.purchaseDictionary.AddAsync(tx, purchase.ID, purchase);
                await tx.CommitAsync();
            }

            return purchase.ID;
        }

        #region StartThread
        private async void LoadTableData()
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                TableQuery<PurchaseTableEntity> query = new TableQuery<PurchaseTableEntity>();

                foreach (PurchaseTableEntity purchase in this._table.ExecuteQuery(query))
                {
                    await this.purchaseDictionary.AddAsync(tx, purchase.ID, new Purchase(purchase));
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
                    var enumerator = (await this.purchaseDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(this._cancellationToken))
                    {
                        Purchase purchase = enumerator.Current.Value;

                        TableOperation insertOperation = TableOperation.InsertOrReplace(new PurchaseTableEntity(purchase));
                        await this._table.ExecuteAsync(insertOperation);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        public async Task<Purchase> GetPurchaseById(long purchaseId)
        {
            using (var tx = this._stateManager.CreateTransaction())
            {
                var purchase = await this.purchaseDictionary.TryGetValueAsync(tx, purchaseId);

                return purchase.Value;
            }
        }
#endregion
    }
}
