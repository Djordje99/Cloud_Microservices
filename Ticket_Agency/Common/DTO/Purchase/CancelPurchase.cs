using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class CancelPurchase
    {
        public string Username { get; set; }
        public long DeaprtureId { get; set; }
        public long PurchaseId { get; set; }
        public long UserPurchaseId { get; set; }
        public double Price { get; set; }
        public int TicketAmount { get; set; }

        public CancelPurchase() { }
    }
}
