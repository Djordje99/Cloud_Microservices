using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class PurchaseTableEntity : TableEntity
    {
        public long ID { get; set; }
        public long DepartureID { get; set; }
        public int TicketPurchaseCount { get; set; }

        public PurchaseTableEntity() { }

        public PurchaseTableEntity(Purchase purchase)
        {
            this.PartitionKey = "Purchase";
            this.RowKey = purchase.ID.ToString();
            this.ID = purchase.ID;
            this.DepartureID = purchase.DepartureID;
            this.TicketPurchaseCount = purchase.TicketPurchaseCount;
        }
    }
}
