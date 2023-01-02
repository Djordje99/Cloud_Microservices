using Common.DTO;
using Common.Enums;
using Common.Interfaces;
using EmailStatelessService.Service;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace EmailStatelessService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class EmailStatelessService : StatelessService
    {
        public EmailStatelessService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");
            List<Departure> departureLis = new List<Departure>();

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var listPurchase = GetDepartureFromEmail();

                        foreach (var mailPurchase in listPurchase)
                        {
                            await validator.BuyDepertureTicket(mailPurchase.Item1, mailPurchase.Item2, mailPurchase.Item3);
                        }

                        Thread.Sleep(3000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private List<Tuple<string, long, int>> GetDepartureFromEmail()
        {
            List<Tuple<string, long, int>> mailPurchase = new List<Tuple<string, long, int>>();

            var mailRepository = new MailRepository("imap.gmail.com", 993, true, "djordje.bozovic.private@gmail.com", "aeishqewfnseuzfk");
            var allEmails = mailRepository.GetUnreadMails();

            foreach (var email in allEmails)
            {
                var emailData = email.Split(',');

                var username = emailData[0].Split(':')[1].Trim();
                var departureId  = Int64.Parse(emailData[1].Split(':')[1].Trim());
                var ticketAmount = Int32.Parse(emailData[2].Split(':')[1].Trim());

                mailPurchase.Add(new Tuple<string, long, int>(username, departureId, ticketAmount));
            }

            return mailPurchase;
        } 
    }
}
