using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class Departure
    {
        public long ID { get; set; }
        [Required]
        [DisplayName("Transport Type")]
        public TransportType TransportType { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        [DisplayName("Departure Start")]
        public DateTime DepartureStart { get; set; }
        [DisplayName("Departure Return")]
        public DateTime? DepartureReturn { get; set; }
        [Required]
        [DisplayName("Deaprture Tickets Count")]
        public int DeaprtureTicketCount { get; set; }
        [Required]
        [DisplayName("Deaprture Avaiable Tickets")]
        public int DepartureAvaiableTicketCount { get; set; }

        public Departure() { }

        public Departure(DepartureTableEntity departure)
        {
            this.ID = Int64.Parse(departure.RowKey);
            this.TransportType = departure.TransportType;
            this.Price = departure.Price;
            this.DepartureStart = departure.DepartureStart;
            this.DepartureReturn = departure.DepartureReturn;
            this.DepartureAvaiableTicketCount = departure.DepartureAvaiableTicketCount;
            this.DeaprtureTicketCount = departure.DeaprtureTicketCount;
        }
    }
}
