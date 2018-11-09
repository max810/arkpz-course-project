using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ARKPZ_CourseWork_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ARKPZ_CourseWork_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BackendContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(BackendContext context, UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task RegisterAsync([FromBody] UserRegisterRequestModel userRegisterRequestModel)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Email = userRegisterRequestModel.Email,
                    UserName = userRegisterRequestModel.Email,
                    Role = userRegisterRequestModel.Role
                };
                var result = await _userManager.CreateAsync(user, userRegisterRequestModel.Password);
                if (result.Succeeded)
                {
                    await Login(new AuthModel
                    {
                        Email = userRegisterRequestModel.Email,
                        Password = userRegisterRequestModel.Password
                    });
                }
                else
                {
                    await Response.WriteAsync("Result validation failed!");
                }
            }
        }

        [HttpPost("login")]
        public async Task Login([FromBody] AuthModel model)
        {
            await _signInManager.PasswordSignInAsync(model.Email, model.Password, true,
                false);

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role),

                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    var jwtSecurityToken = new JwtSecurityToken(
                        issuer: AuthOptions.ISSUER,
                        audience: AuthOptions.AUDIENCE,
                        claims: claims,
                        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthOptions.KEY)),
                            SecurityAlgorithms.HmacSha256));
                    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                    var response = new
                    {
                        access_token = encodedJwt,
                        username = user.UserName
                    };

                    Response.ContentType = "application/json";
                    await Response.WriteAsync(JsonConvert.SerializeObject(response,
                        new JsonSerializerSettings { Formatting = Formatting.Indented }));
                    return;
                }

                await Response.WriteAsync("Wrong credentials!");
            }
        }

        [HttpPost("logout")]
        public async Task LogOff()
        {
            await _signInManager.SignOutAsync();
        }

        //[HttpPost("uploadPhoto")]
        //[Authorize]
        //public async Task UploadPhotoAsync(IFormFile file)
        //{
        //    var email = User.Identity.Name;
        //    var user = _context
        //        .Users
        //        .Include(usr => usr.Photos)
        //        .FirstOrDefault(u => u.Email == email);
        //    using (var stream = new MemoryStream())
        //    {
        //        await file.CopyToAsync(stream);
        //        var result = stream.ToArray();
        //        var photo = new Photo { Content = result };
        //        user.Photos.Add(photo);
        //        await _context.SaveChangesAsync();
        //    }
        //}

        //[HttpGet("getPhoto")]
        //public async Task<byte[]> GetPhotoAsync(string id)
        //{

        //    var photo = await _context.Photo.FirstOrDefaultAsync(ph => ph.Id == id);
        //    Response.ContentType = "image/jpeg";
        //    return photo.Content;
        //}

        //[HttpGet("getPhotoTuc")]
        //public async Task<IActionResult> GetPhotoTuc(string id)
        //{
        //    var image = await _context.Photo.FirstOrDefaultAsync(ph => ph.Id == id);
        //    return File(image.Content, "image/jpeg");
        //}


    }
}


//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using ARKPZ_CourseWork_Backend.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using Newtonsoft.Json;

//namespace ARKPZ_CourseWork_Backend.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private Dictionary<int, JwtSecurityToken> tokens = new Dictionary<int, JwtSecurityToken>();
//        private BackendContext context;
//        public AuthController(BackendContext _context)
//        {
//            context = _context;
//        }


//        [HttpPost("authenticate")]
//        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
//        public ActionResult Login(string email, string password)
//        {
//            var identity = GetIdentity(email, password);
//            if (identity == null)
//            {
//                return BadRequest("Invalid username or password.");
//            }

//            var now = DateTime.UtcNow;
//            // создаем JWT-токен
//            var jwt = new JwtSecurityToken(
//                    issuer: AuthOptions.ISSUER,
//                    audience: AuthOptions.AUDIENCE,
//                    notBefore: now,
//                    claims: identity.Claims,
//                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
//                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
//            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

//            var response = new
//            {
//                access_token = encodedJwt,
//                userName = identity.Name,
//                profile_type = identity.Claims.Where(x => x.Type == "profile_type").FirstOrDefault().Value
//            };

//            // сериализация ответа
//            //Response.ContentType = "application/json";
//            //string responseJson = JsonConvert.SerializeObject(response);

//            return Ok(response);
//        }

//        private ClaimsIdentity GetIdentity(string email, string password)
//        {
//            var users = context.Users;
//            var employees = context.Employees;

//            //var user = users.FirstOrDefault(x => x.Email == email
//            //&& password.GetHashCode(StringComparison.InvariantCultureIgnoreCase).ToString() == x.Password);

//            User user = users.FirstOrDefault(x => x.Email == email
//            && password == x.Password);

//            User employee = employees.FirstOrDefault(x => x.Email == email
//            && password == x.Password);
//            User user = user ?? employee;
//            if (user == null)
//            {
//                return null;
//            }

//            var claims = new List<Claim>
//                {
//                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
//                    new Claim("profile_type", user == user ? "user" : "employee")
//                };
//            ClaimsIdentity claimsIdentity =
//            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
//                ClaimsIdentity.DefaultRoleClaimType);

//            return claimsIdentity;
//        }
//    }
//}

