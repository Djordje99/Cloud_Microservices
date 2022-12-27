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
            bool isBought = false;

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddressBank = new EndpointAddress("net.tcp://localhost:20025/BankService");
            var endpointAddressDeparture = new EndpointAddress("net.tcp://localhost:20015/DepartureService");

            using (var channelFactoryBank = new ChannelFactory<IBankService>(binding, endpointAddressBank))
            {
                IBankService bank = null;
                using (var channelFactoryDeparture = new ChannelFactory<IDepartureService>(binding, endpointAddressDeparture))
                {
                    IDepartureService departure = null;
                    try
                    {
                        departure = channelFactoryDeparture.CreateChannel();
                        bank = channelFactoryBank.CreateChannel();

                        var departureResult = await departure.EnlistTicketPurchase(departureId, ticketAmount);

                        var price = await departure.GetPrice(departureId, ticketAmount);

                        var bankResult = await bank.EnlistMoneyTransfer(username, price);

                        if (departureResult && bankResult)
                        {
                            await departure.Commit();
                            await bank.Commit();
                            isBought = true;
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
                        isBought = false;
                    }
                }
            }

            return isBought;
        }
    }
}
