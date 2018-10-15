using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARKPZ_CourseWork_Backend.Models
{
    public class Drone
    {
        // unnecessary for Id
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "ok";
        //public double Speed { get; set; }

        public override string ToString()
        {
            return $"{Id}, ({Latitude}, {Longitude}) : {Status}";
        }

        internal object GetApproximateArrivalTime(Coordinates coords)
        {
            throw new NotImplementedException();
        }
    }
}
