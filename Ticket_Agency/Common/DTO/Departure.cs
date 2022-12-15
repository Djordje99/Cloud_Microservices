using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class Departure
    {
        public int ID { get; set; }
        public TransportType TransportType { get; set; }
        public double Price { get; set; }
        public DateTime DepartureStart { get; set; }
        public DateTime? DepartureReturn { get; set; }
        public int DeaprtureTicketCount { get; set; }
        public int DepartureAvaiableTicketCount { get; set; }
    }
}
