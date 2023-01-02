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
    public interface ITransactionCordinatorService
    {
        [OperationContract]
        Task<bool> BuyDepertureTicket(string username, long departureId, int ticketAmount);

        [OperationContract]
        Task SetDictionary();

        [OperationContract]
        Task<Purchase> GetPurchaseById(long purchaseId);

        [OperationContract]
        Task CancelPurchase(long purchaseId);
    }
}
