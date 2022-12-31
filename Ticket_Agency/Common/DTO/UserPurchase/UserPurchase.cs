using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class UserPurchase
    {
        [DataMember]
        public long ID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public long PurchaseId { get; set; }

        public UserPurchase() { }

        public UserPurchase(UserPurchaseTableEntity user)
        {
            this.Username = user.Username;
            this.PurchaseId = user.PurchaseId;
            this.ID = user.ID;
        }
    }
}
