using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Weather
{
    [DataContract]
    public class WeatherData
    {
        [DataMember]
        public double Temperature { get; set; }
        [DataMember]
        public string IconLocation { get; set; }

        public WeatherData() { }
    }
}
