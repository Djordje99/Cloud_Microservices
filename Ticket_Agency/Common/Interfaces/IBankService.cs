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
    public interface IBankService : ITransactionService
    {
        [OperationContract]
        Task<bool> CreateBankAccount(BankAccount account);

        [OperationContract]
        Task<bool> EnlistMoneyTransfer(long accountNumber, double price);

        [OperationContract]
        Task SetDictionary();
    }
}
