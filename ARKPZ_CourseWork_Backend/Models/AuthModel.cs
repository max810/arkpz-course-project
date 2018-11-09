using System.ComponentModel.DataAnnotations;

namespace ARKPZ_CourseWork_Backend.Models
{
    public class AuthModel
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}