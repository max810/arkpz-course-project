using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARKPZ_CourseWork_Backend.Models
{
    public class User: IdentityUser
    {
        //public int Id { get; set; }
        //public string Email { get; set; }
        //public string Password { get; set; }
        //public string UserName { get; internal set; }
        public string Role { get; set; }
    }
}
