using Common.DTO;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Azure.ServiceBus;

namespace ValidatorStatelessService.Services
{
    public class ValidatorService : IValidatorService
    {
        public async Task BuyDepertureTicket(string username, long departureId, int ticketAmount)
        {
            bool isBought = false;

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20035/TransactionCordinatorService");

            using (var channelFactory = new ChannelFactory<ITransactionCordinatorService>(binding, endpointAddress))
            {
                ITransactionCordinatorService transactionService = null;
                try
                {
                    transactionService = channelFactory.CreateChannel();
                    isBought = await transactionService.BuyDepertureTicket(username, departureId, ticketAmount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (isBought)
            {
                //include pub sub 
                Console.WriteLine(isBought);
            }
        }

        public async Task CreateBankAccount(BankAccount account)
        {
            bool isCreated = false;

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20025/BankService");

            using (var channelFactory = new ChannelFactory<IBankService>(binding, endpointAddress))
            {
                IBankService bankService = null;
                try
                {
                    bankService = channelFactory.CreateChannel();
                    isCreated = await bankService.CreateBankAccount(account);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (isCreated)
            {
                //include pub sub 
                Console.WriteLine(isCreated);
            }
        }

        public async Task CreateDeparture(Departure departure)
        {
            //validate deaprture
            departure.DepartureAvaiableTicketCount = departure.DeaprtureTicketCount;


            bool isCreated = false;

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20015/DepartureService");

            using (var channelFactory = new ChannelFactory<IDepartureService>(binding, endpointAddress))
            {
                IDepartureService departureService = null;
                try
                {
                    departureService = channelFactory.CreateChannel();
                    isCreated = await departureService.CreateDeparture(departure);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (isCreated)
            {
                //include pub sub 
                Console.WriteLine(isCreated);
            }
        }

        public async Task<List<Departure>> ListDeparture()
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20015/DepartureService");
            List<Departure> departureList = new List<Departure>();

            using (var channelFactory = new ChannelFactory<IDepartureService>(binding, endpointAddress))
            {
                IDepartureService departureService = null;
                try
                {
                    departureService = channelFactory.CreateChannel();
                    departureList = await departureService.ListDeparture();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return departureList;
        }

        public async Task<List<Departure>> ListDepartureFilter(string transportType, DateTime fromDate, int availableTickets)
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20015/DepartureService");
            List<Departure> departureList = new List<Departure>();

            using (var channelFactory = new ChannelFactory<IDepartureService>(binding, endpointAddress))
            {
                IDepartureService departureService = null;
                try
                {
                    departureService = channelFactory.CreateChannel();
                    departureList = await departureService.ListDepartureFilter(transportType, fromDate, availableTickets);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return departureList;
        }

        public async Task CancelPurchase(CancelPurchase cancelPurchase)
        {
            long bankAccount = 0;
            //get userpurchases
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20001/UserService");

            using (var channelFactory = new ChannelFactory<IUserService>(binding, endpointAddress))
            {
                IUserService userService = null;
                try
                {
                    userService = channelFactory.CreateChannel();
                    await userService.CancelPurchase(cancelPurchase.UserPurchaseId);
                    bankAccount = await userService.GetUsersBankAcount(cancelPurchase.Username);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //give back money
            binding = new NetTcpBinding(SecurityMode.None);
            endpointAddress = new EndpointAddress("net.tcp://localhost:20025/BankService");

            using (var channelFactory = new ChannelFactory<IBankService>(binding, endpointAddress))
            {
                IBankService bankService = null;
                try
                {
                    bankService = channelFactory.CreateChannel();
                    await bankService.CancelPurchase(bankAccount, cancelPurchase.Price);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //get purchases
            binding = new NetTcpBinding(SecurityMode.None);
            endpointAddress = new EndpointAddress("net.tcp://localhost:20035/TransactionCordinatorService");

            using (var channelFactory = new ChannelFactory<ITransactionCordinatorService>(binding, endpointAddress))
            {
                ITransactionCordinatorService transactionService = null;
                try
                {
                    transactionService = channelFactory.CreateChannel();

                    await transactionService.CancelPurchase(cancelPurchase.PurchaseId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //get deaprtures
            binding = new NetTcpBinding(SecurityMode.None);
            endpointAddress = new EndpointAddress("net.tcp://localhost:20015/DepartureService");

            using (var channelFactory = new ChannelFactory<IDepartureService>(binding, endpointAddress))
            {
                IDepartureService departureService = null;
                try
                {
                    departureService = channelFactory.CreateChannel();

                    await departureService.CancelPurchase(cancelPurchase.DeaprtureId, cancelPurchase.TicketAmount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task<List<DetailPurchase>> PurchaseList(string username)
        {
            List<UserPurchase> userPurchases = new List<UserPurchase>();
            List<Purchase> purchases = new List<Purchase>();
            List<Departure> departureList = new List<Departure>();
            List<DetailPurchase> detailPurchases = new List<DetailPurchase>();

            //get userpurchases
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20001/UserService");

            using (var channelFactory = new ChannelFactory<IUserService>(binding, endpointAddress))
            {
                IUserService userService = null;
                try
                {
                    userService = channelFactory.CreateChannel();
                    userPurchases = await userService.GetUserPurchases(username);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //get purchases
            binding = new NetTcpBinding(SecurityMode.None);
            endpointAddress = new EndpointAddress("net.tcp://localhost:20035/TransactionCordinatorService");

            using (var channelFactory = new ChannelFactory<ITransactionCordinatorService>(binding, endpointAddress))
            {
                ITransactionCordinatorService transactionService = null;
                try
                {
                    transactionService = channelFactory.CreateChannel();

                    foreach (var userPurchase in userPurchases)
                    {
                        var purchase = await transactionService.GetPurchaseById(userPurchase.PurchaseId);

                        purchases.Add(purchase);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //get deaprtures
            binding = new NetTcpBinding(SecurityMode.None);
            endpointAddress = new EndpointAddress("net.tcp://localhost:20015/DepartureService");

            using (var channelFactory = new ChannelFactory<IDepartureService>(binding, endpointAddress))
            {
                IDepartureService departureService = null;
                try
                {
                    departureService = channelFactory.CreateChannel();

                    foreach (var purchase in purchases)
                    {
                        var departure = await departureService.GetDepartureById(purchase.DepartureID);

                        departureList.Add(departure);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            for (int i = 0; i < departureList.Count; i++)
            {
                DetailPurchase detailPurchase = new DetailPurchase()
                {
                    DepartureID = departureList[i].ID,
                    Username = username,
                    PurchaseID = purchases[i].ID,
                    CityName = departureList[i].CityName,
                    DepartureStart = departureList[i].DepartureStart,
                    DepartureReturn = departureList[i].DepartureReturn,
                    Price = departureList[i].Price * purchases[i].TicketPurchaseCount,
                    TicketAmount = purchases[i].TicketPurchaseCount,
                    UserPurchaseID = userPurchases[i].ID
                };

                detailPurchases.Add(detailPurchase);
            }

            return detailPurchases;
        }

        public async Task ValidateUserLogIn(RegisterUser user)
        {
            bool isLoged = false;

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20001/UserService");

            using (var channelFactory = new ChannelFactory<IUserService>(binding, endpointAddress))
            {
                IUserService userService = null;
                try
                {
                    userService = channelFactory.CreateChannel();
                    isLoged = await userService.LogIn(user.Username, user.Password);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (isLoged)
            {
                //include pub sub 
                Console.WriteLine(isLoged);
            }
        }

        public async Task ValidateUserRegister(RegisterUser user)
        {
            bool isRegistred = false;

            ValidateUser(user);

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20001/UserService");

            using (var channelFactory = new ChannelFactory<IUserService>(binding, endpointAddress))
            {
                IUserService userService = null;
                try
                {
                    userService = channelFactory.CreateChannel();
                    isRegistred = await userService.Register(user);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (isRegistred)
            {
                Console.WriteLine(isRegistred);
                //pub sub to client
            }
        }

        private void ValidateUser(RegisterUser user)
        {
            MailAddress addr = new MailAddress(user.Email);

            if (!user.Password.Equals(user.PasswordRepeat))
                throw new Exception("Password and Repeat password are not same.");
        }
    }
}
