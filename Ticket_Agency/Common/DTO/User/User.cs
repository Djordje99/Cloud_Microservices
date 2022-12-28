using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class User
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public long AccountNumber { get; set; }
        [DataMember]
        public List<long> PurchaseHistory { get; set; }

        public User() { }

        public User(UserTableEntity user)
        {
            this.Username = user.Username;
            this.Email = user.Email;
            this.Password = user.Password;
            this.AccountNumber = user.AccountNumber;
            this.PurchaseHistory = user.PurchaseHistory;
        }

        public User(RegisterUser user)
        {
            this.Username = user.Username;
            this.Email = user.Email;
            this.Password = user.Password;
            this.AccountNumber = user.AccountNumber;
            this.PurchaseHistory = new List<long>();
        }
    }
}
