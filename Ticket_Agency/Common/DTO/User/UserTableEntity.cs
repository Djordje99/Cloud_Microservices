using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class UserTableEntity : TableEntity
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public List<long> PurchaseHistory { get; set; }
        [DataMember]
        public long AccountNumber { get; set; }

        public UserTableEntity(User user)
        {
            this.PartitionKey = "User";
            this.RowKey = user.Username;
            this.Username = user.Username;
            this.Email = user.Email;
            this.Password = user.Password;
            this.AccountNumber = user.AccountNumber;

            if (user.PurchaseHistory == null)
                this.PurchaseHistory = new List<long>();
            else
                this.PurchaseHistory = user.PurchaseHistory;
        }

        public UserTableEntity() { }
    }
}
