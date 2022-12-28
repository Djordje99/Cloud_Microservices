using Common.DTO;
using Common.Interfaces;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using UserStatefulService.Services;

namespace UserStatefulService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class UserStatefulService : StatefulService
    {
        public UserStatefulService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener>(1) {
                new ServiceReplicaListener(context => this.CreateWcfCommunicationListener(context)),
            };
        }

        private ICommunicationListener CreateWcfCommunicationListener(StatefulServiceContext context)
        {
            string host = context.NodeContext.IPAddressOrFQDN;

            var endpointConfig = context.CodePackageActivationContext.GetEndpoint("UserService");
            int port = endpointConfig.Port;
            var scheme = endpointConfig.Protocol.ToString();
            string url = string.Format(CultureInfo.InvariantCulture, "net.{0}://{1}:{2}/UserService", scheme, host, port);

            WcfCommunicationListener<IUserService> listenet = null;

            listenet = new WcfCommunicationListener<IUserService>(
            serviceContext: context,
            wcfServiceObject: new UserService(this.StateManager),
            listenerBinding: WcfUtility.CreateTcpListenerBinding(maxMessageSize: 1024 * 1024 * 1024),
            address: new System.ServiceModel.EndpointAddress(url));

            ServiceEventSource.Current.Message("Listener created. On url " + url);

            return listenet;
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20001/UserService");

            using (var channelFactory = new ChannelFactory<IUserService>(binding, endpointAddress))
            {
                IUserService userService = null;
                try
                {
                    userService = channelFactory.CreateChannel();
                    await userService.SetDictionary();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
