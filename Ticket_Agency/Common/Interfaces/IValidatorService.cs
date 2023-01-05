using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IValidatorService
    {
        [OperationContract]
        Task ValidateUserRegister(RegisterUser user);

        [OperationContract]
        Task ValidateUserLogIn(RegisterUser user);

        [OperationContract]
        Task CreateDeparture(Departure departure);

        [OperationContract]
        Task<List<Departure>> ListDeparture();

        [OperationContract]
        Task CreateBankAccount(BankAccount account);

        [OperationContract]
        Task BuyDepertureTicket(string username, long departureId, int ticketAmount);

        [OperationContract]
        Task<List<Departure>> ListDepartureFilter(string transportType, DateTime fromDate, int availableTickets);

        [OperationContract]
        Task<List<DetailPurchase>> PurchaseList(string username);

        [OperationContract]
        Task CancelPurchase(CancelPurchase cancelPurchase);

        [OperationContract]
        Task<List<Departure>> ListHistoryDeparture();
    }
}
