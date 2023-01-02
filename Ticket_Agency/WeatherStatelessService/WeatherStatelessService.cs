using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Fabric;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using WeatherStatelessService.Services;

namespace WeatherStatelessService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WeatherStatelessService : StatelessService
    {
        public WeatherStatelessService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>(1) {
                new ServiceInstanceListener(context => this.CreateWcfCommunicationListenet(context)),
            };
        }

        private ICommunicationListener CreateWcfCommunicationListenet(StatelessServiceContext context)
        {
            string host = context.NodeContext.IPAddressOrFQDN;

            var endpointConfig = context.CodePackageActivationContext.GetEndpoint("WeatherService");
            int port = endpointConfig.Port;
            var scheme = endpointConfig.Protocol.ToString();
            string url = string.Format(CultureInfo.InvariantCulture, "net.{0}://{1}:{2}/WeatherService", scheme, host, port);

            WcfCommunicationListener<IWeatherService> listenet = null;

            listenet = new WcfCommunicationListener<IWeatherService>(
            serviceContext: context,
            wcfServiceObject: new WeatherService(),
            listenerBinding: WcfUtility.CreateTcpListenerBinding(maxMessageSize: 1024 * 1024 * 1024),
            address: new System.ServiceModel.EndpointAddress(url));

            ServiceEventSource.Current.Message("Listener created. On url " + url);

            return listenet;
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
           
        }
    }
}
