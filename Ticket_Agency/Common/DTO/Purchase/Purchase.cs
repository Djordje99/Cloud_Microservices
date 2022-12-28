using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class Purchase
    {
        [DataMember]
        public long ID { get; set; }
        [DataMember]
        public long DepartureID { get; set; }
        [DataMember]
        public int TicketPurchaseCount { get; set; }

        public Purchase() { }

        public Purchase(PurchaseTableEntity purchase)
        {
            this.ID = purchase.ID;
            this.DepartureID = this.DepartureID;
            this.TicketPurchaseCount = this.TicketPurchaseCount;
        }
    }
}
