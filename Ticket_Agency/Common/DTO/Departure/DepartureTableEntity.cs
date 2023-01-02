using Common.Enums;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class DepartureTableEntity : TableEntity
    {
        [DataMember]
        public long ID { get; set; }
        [DataMember]
        public string CityName { get; set; }
        [DataMember]
        public TransportType TransportType { get; set; }
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public DateTime DepartureStart { get; set; }
        [DataMember]
        public DateTime? DepartureReturn { get; set; }
        [DataMember]
        public int DeaprtureTicketCount { get; set; }
        [DataMember]
        public int DepartureAvaiableTicketCount { get; set; }
        [DataMember]
        public int TransportTypeInt { get; set; }

        public DepartureTableEntity() { }

        public DepartureTableEntity(Departure departure)
        {
            this.PartitionKey = "Departure";
            this.RowKey = departure.ID.ToString();
            this.ID = departure.ID;
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
