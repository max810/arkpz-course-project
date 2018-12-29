using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARKPZ_CourseWork_Backend.Models
{
    public struct DroneResponse
    {
        [JsonProperty(PropertyName = "eta")]
        public TimeSpan ETA { get; private set; }
        [JsonProperty(PropertyName = "speed_kmh")]
        public int SpeedKMH { get; private set; }

        public DroneResponse(TimeSpan eta, int speedKMH) : this()
        {
            ETA = eta;
            SpeedKMH = speedKMH;
        }
    }
}
