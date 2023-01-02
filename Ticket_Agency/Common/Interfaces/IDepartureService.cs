using Common.DTO;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IDepartureService : ITransactionService
    {
        [OperationContract]
        Task<bool> CreateDeparture(Departure departure);

        [OperationContract]
        Task<List<Departure>> ListDeparture();

        [OperationContract]
        Task<bool> EnlistTicketPurchase(long departureId, int ticketAmount);

        [OperationContract]
        Task<double> GetPrice();

        [OperationContract]
        Task SetDictionary();

        [OperationContract]
        Task<List<Departure>> ListDepartureFilter(string transportType, DateTime fromDate, int availableTickets);

        [OperationContract]
        Task<Departure> GetDepartureById(long departureID);

        [OperationContract]
        Task CancelPurchase(long deaprtureId, int ticketAmount);
    }
}
