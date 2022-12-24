using Common.DTO;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ValidatorStatelessService.Services
{
    public class ValidatorService : IValidatorService
    {
        public async Task ValidateUserRegister(RegisterUser user)
        {
            //validate object
            //and create user object

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20001/UserService");

            using (var channelFactory = new ChannelFactory<IUserService>(binding, endpointAddress))
            {
                IUserService userService = null;
                try
                {
                    userService = channelFactory.CreateChannel();
                    var result = await userService.Register(new UserDict(user));
                    Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
