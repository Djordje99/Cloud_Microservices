using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class DetailPurchase
    {
        [DataMember]
        public long DepartureID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public long PurchaseID { get; set; }
        [DataMember]
        public long UserPurchaseID { get; set; }
        [DataMember]
        public string CityName { get; set; }
        [DataMember]
        public DateTime DepartureStart { get; set; }
        [DataMember]
        public DateTime? DepartureReturn { get; set; }
        [DataMember]
        public int TicketAmount { get; set; }
        [DataMember]
        public double Price { get; set; }

        public DetailPurchase() { }
    }
}
