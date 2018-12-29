using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARKPZ_CourseWork_Backend.Models
{
    public class CrashRecord
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Drone AssignedDrone { get; set; }
        public Coordinates Coords { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public CrashRecord()
        {
            
        }
    }
}
