using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class Purchase
    {
        public int ID { get; set; }
        public int DeaprtureID { get; set; }
        public int UserID { get; set; }
        public int TicketPurchaseCount { get; set; }
    }
}
