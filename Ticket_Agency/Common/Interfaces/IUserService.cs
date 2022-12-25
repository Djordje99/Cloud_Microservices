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
        Task<bool> Register(UserDict user);

        [OperationContract]
        Task<bool> LogIn(string username, string password);
    }
}
