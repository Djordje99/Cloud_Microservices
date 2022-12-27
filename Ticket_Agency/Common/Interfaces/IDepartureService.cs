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
    public interface IDepartureService : ITransactionService
    {
        [OperationContract]
        Task<bool> CreateDeparture(Departure departure);

        [OperationContract]
        Task<List<Departure>> ListDeparture();
    }
}
