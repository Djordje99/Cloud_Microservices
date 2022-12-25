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
        public int ID { get; set; }
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

        public DepartureTableEntity() { }

    }
}
