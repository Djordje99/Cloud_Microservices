using Common.DTO;
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
    }
}
