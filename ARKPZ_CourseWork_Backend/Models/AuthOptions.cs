using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARKPZ_CourseWork_Backend.Models
{
    public class AuthOptions
    {
        public const string ISSUER = "ACCR_system"; // издатель токена
        public const string AUDIENCE = "localhost_user"; // потребитель токена
        public const string KEY = "mysupersecret_secretkey123";   // ключ для шифрации
        public const int LIFETIME = 1; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
