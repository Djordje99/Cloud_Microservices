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
    public interface IUserService
    {
        [OperationContract]
        Task<bool> Register(RegisterUser user);

        [OperationContract]
        Task<bool> LogIn(string username, string password);

        [OperationContract]
        Task<long> GetUsersBankAcount(string username);

        [OperationContract]
        Task SetDictionary();

        [OperationContract]
        Task SetPurchaseToUser(string username, long purchaseID);

        [OperationContract]
        Task<List<UserPurchase>> GetUserPurchases(string username);

        [OperationContract]
        Task CancelPurchase(long userPurchaseId);
    }
}
