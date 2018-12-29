using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ARKPZ_CourseWork_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Net;
//using System.Web.Script.Serialization;

namespace ARKPZ_CourseWork_Backend.Controllers
{
    [Route("accr/[controller]")]
    [ApiController]
    public class CrashController : ControllerBase
    {
        // POST api/values
        private IConfiguration _configuration;
        private Dictionary<int, string> DroneAddresses = new Dictionary<int, string> { };
        private readonly UserManager<User> _userManager;
        private readonly BackendContext dbContext;
        public CrashController(BackendContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            dbContext = context;
            _userManager = userManager;
            _configuration = configuration;
            //var drone = new Drone()
            //{
            //    Id = 0,
            //    Latitude = 4.5,
            //    Longitude = 1.24543,
            //    Status = "Ok nigga"
            //};
            //dbContext.Drones.Add(drone);
            //Drone = drone;
            //dbContext.SaveChanges();
        }

        [HttpPost("send-crash")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> Crash([FromBody] CrashReport crashReport)
        {
            //int userId = crashReport.UserId;
            string email = User.Identity.Name;
            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            //var user = dbContext.Users.FirstOrDefault(x => x.Id == userId.ToString());
            //if (user is null)
            //{
            //    return Unauthorized();
            //}
            //if (user.TrustLevel < 5)
            //{
            //    return Ok("untrustworthy");
            //}
            var crashRecord = new CrashRecord()
            {
                User = user,
                Coords = crashReport.Coords,
            };
            Drone nearestDrone = GetNearestDrone(crashRecord.Coords);
            if (nearestDrone == null)
            {
                return Ok(new
                {
                    drone = (Drone)null,
                    eta = -1,
                });
            }
            crashRecord.AssignedDrone = nearestDrone;

            dbContext.CrashRecords.Add(crashRecord);
            dbContext.SaveChanges();


            try
            {
                TimeSpan arrival = await GetETA(crashRecord.Coords); /*GetArrivalTimeTest(nearestDrone.Id, crashReport.Coords);*/
                var response = new
                {
                    drone = nearestDrone,
                    eta = arrival
                };
                return Ok(response);

            }
            catch (DroneNotAvailableException)
            {
                var response = new
                {
                    drone = (Drone)null,
                    eta = TimeSpan.FromSeconds(0)
                };
                return Ok(response);
            }
          
            //TimeSpan arrival = GetArrivalTimeTest(nearestDrone.Id, crashReport.Coords);
            //return new JsonResult(new object()) { StatusCode = 200 };
        }

        private Drone GetNearestDrone(Coordinates coords)
        {
            var drones = dbContext.Drones;
            var nearestDrone = drones.Where(x => x.Status == "ok")
                .OrderBy(x => x.GetDistance(coords))
                .FirstOrDefault();

            return nearestDrone;
        }

        private async Task<TimeSpan> GetETA(Coordinates coords)
        {
            // тут буде зберігатися вся інформація про поточну операцію
            string output = "\n\n============== API Call ==================\n\n";
            try
            {
                // створюємо http клієнт для взаємодії з IoT пристрієм.
                HttpClient iotApi = new HttpClient();
                // отримуємо необхідні дані для підключення із конфігураційного файлу.
                var section = _configuration.GetSection("IoT");
                string iotIp = section["IoT-IP"];
                string iotPort = section["IoT-Port"];
                // формуємо строку запиту
                string apiRequestUrl = $"http://{iotIp}:{iotPort}/api?lat={coords.Latitude}&lon={coords.Longitude}";
                output += $"API request url: {apiRequestUrl}\n";
                // робимо GET запит до IoT пристрою
                using (var response = await iotApi.GetAsync(apiRequestUrl))
                {
                    output += $"RESPONSE CODE: {response.StatusCode}\n";
                    // перевіряємо успішність запиту
                    if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                    {
                        throw new DroneNotAvailableException("Drone is too far away to arrive");
                    }
                    // отримуємо тіло відповіді
                    var content = response.Content;
                    string contentStr = await content.ReadAsStringAsync();
                    // перетворюємо результат у об'єкт, що являє собою проміжок часу.
                    TimeSpan result = TimeSpan.Parse(contentStr);
                    /*JsonConvert.DeserializeObject<DroneResponse>(contentStr);*/

                    output += "\n==============END API Call==============";

                    return result;
                }
            }
            finally
            {
                // пишемо у консоль увесь результат операції, незалежно від успішності/неуспішності запиту
                Console.WriteLine(output + "\n\n");
            }
        }

        //[Authorize]
        [HttpGet("test")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public string Test()
        {
            var drones = dbContext.Drones;
            return string.Join("\n", drones);
        }

        [HttpGet("stat")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<CrashRecord>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CrashRecord>>> GetStatistics()
        {
            string email = User.Identity.Name;
            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

            return Ok(GetUserStat(user));
        }

        [HttpGet("stat/{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(IEnumerable<CrashRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CrashRecord>>> GetStatistics([FromBody] string email)
        {
            User user = await _userManager.FindByEmailAsync(email);
            //User user = dbContext.Users.FirstOrDefault(x => x.Id == id.ToString());
            if (user == null)
            {
                Response.StatusCode = 400;
                return BadRequest($"User with email {email} not found");
            }

            return Ok(GetUserStat(user));
        }

        private TimeSpan GetArrivalTimeTest(int droneId, Coordinates coords)
        {
            //string droneAddress = DroneAddresses[droneId];
            //var socket = new WebSocket(droneAddress);
            //socket.OnMessage += OnDroneMessageReceivedTest;
            //string requestFormatted = FormatArrivalTimeRequestTest(coords);
            ////socket.Send(requestFormatted);

            return TimeSpan.FromMinutes(10);
        }

        private string FormatArrivalTimeRequestTest(Coordinates coords)
        {
            var request = new
            {
                Longitude = coords.Longitude,
                Latitude = coords.Latitude
            };

            return JsonConvert.SerializeObject(request);
        }

        //private void OnDroneMessageReceivedTest(object sender, MessageEventArgs e)
        //{
        //    var data = JsonConvert.DeserializeObject<DateTime>(e.Data);
        //    arrivalTime = data;
        //}

        private IEnumerable<CrashRecord> GetUserStat(User user)
        {
            var crashes = dbContext.CrashRecords
                .Include(x => x.AssignedDrone)
                .Where(x => x.User.Id == user.Id);
            //var crashCount = crashes.Count();
            return crashes;
        }
    }
}
