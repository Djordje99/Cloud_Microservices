using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class UserDict
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<int> PurchaseHistory { get; set; }
        public int BankAccountID { get; set; }

        public UserDict(RegisterUser user)
        {
            this.Username = user.Username;
            this.Email = user.Email;
            this.Password = user.Password;
            this.BankAccountID = user.BankAccountID;
        }
        public UserDict() { }
    }
}
