using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARKPZ_CourseWork_Backend.Models
{
    [Owned]
    public class Coordinates
    {
        public double Longitude;
        public double Latitude;
    }
}