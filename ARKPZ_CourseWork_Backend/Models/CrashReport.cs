using System;

namespace ARKPZ_CourseWork_Backend.Models
{
    public class CrashReport
    {
        //public string UserEmail { get; set; }
        public DangerLevel DangerLevel {get;set;}
        public Coordinates Coords { get; set; }
    }
}