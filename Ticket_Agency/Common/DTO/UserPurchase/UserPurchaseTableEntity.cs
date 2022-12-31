using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class UserPurchaseTableEntity : TableEntity
    {
        public long ID { get; set; }
        public string Username { get; set; }
        public long PurchaseId { get; set; }

        public UserPurchaseTableEntity() { }

        public UserPurchaseTableEntity(UserPurchase user)
        {
            this.PartitionKey = "UserPurchase";
            this.RowKey = user.ID.ToString();
            this.Username = user.Username;
            this.PurchaseId = user.PurchaseId;
            this.ID = user.ID;
        }
    }
}
