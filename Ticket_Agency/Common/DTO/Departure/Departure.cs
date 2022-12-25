using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class Departure
    {
        public int ID { get; set; }
        [Required]
        public TransportType TransportType { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public DateTime DepartureStart { get; set; }
        public DateTime? DepartureReturn { get; set; }
        [Required]
        public int DeaprtureTicketCount { get; set; }
        [Required]
        public int DepartureAvaiableTicketCount { get; set; }

        public Departure() { }
    }
}
