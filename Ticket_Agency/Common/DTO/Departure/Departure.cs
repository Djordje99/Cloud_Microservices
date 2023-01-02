using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class Departure
    {
        [DataMember]
        public long ID { get; set; }

        [Required]
        [DataMember]
        [DisplayName("City Name")]
        public string CityName { get; set; }

        [Required]
        [DisplayName("Transport Type")]
        [DataMember]
        public TransportType TransportType { get; set; }

        [Required]
        [DataMember]
        public double Price { get; set; }

        [Required]
        [DisplayName("Departure Start")]
        [DataMember]
        public DateTime DepartureStart { get; set; }

        [DisplayName("Departure Return")]
        [DataMember]
        public DateTime? DepartureReturn { get; set; }

        [Required]
        [DisplayName("Deaprture Tickets Count")]
        [DataMember]
        public int DeaprtureTicketCount { get; set; }

        [Required]
        [DisplayName("Deaprture Avaiable Tickets")]
        [DataMember]
        public int DepartureAvaiableTicketCount { get; set; }

        public int TransportTypeInt { get; set; }

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
            this.TransportTypeInt = departure.TransportTypeInt;
            this.CityName = departure.CityName;
        }
    }
}
