using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARKPZ_CourseWork_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
//using System.Web.Script.Serialization;

namespace ARKPZ_CourseWork_Backend.Controllers
{
    [Route("accr/[controller]")]
    [ApiController]
    public class CrashController : ControllerBase
    {
        // POST api/values
        private Dictionary<int, string> DroneAddresses = new Dictionary<int, string> {};
        private readonly UserManager<User> _userManager;
        private readonly BackendContext dbContext;
        private DateTime arrivalTime;
        public CrashController(BackendContext context, UserManager<User> userManager)
        {
            dbContext = context;
            _userManager = userManager;
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
            if (nearestDrone is null)
            {
                return Ok("no drones available");
                // handle this case
                // return "No drones available, but we will call ambulance"
            }
            crashRecord.AssignedDrone = nearestDrone;

            // ?
            var response = new
            {
                DroneId = nearestDrone.Id,
                DroneLongitude = nearestDrone.Longitude,
                DroneLatitude = nearestDrone.Latitude,
                ApproximateArrivalTime = nearestDrone.GetApproximateArrivalTime(crashRecord.Coords)
                // TODO mean speed
            };
            dbContext.CrashRecords.Add(crashRecord);
            dbContext.SaveChanges();
            TimeSpan arrival = GetArrivalTimeTest(nearestDrone.Id, crashReport.Coords);
            return Ok($"ETA: {arrival.Minutes} minutes");
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

        //[Authorize]
        [HttpGet("test")]
        public string Test()
        {
            var drones = dbContext.Drones;
            return string.Join("\n", drones);
        }

        [HttpGet("stat")]
        [Authorize]
        public async Task<ActionResult<string>> GetStatistics()
        {
            string email = User.Identity.Name;
            User user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

            return GetUserStat(user);
        }

        [HttpGet("stat/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<string>> GetStatistics([FromBody] string email)
        {
            User user = await _userManager.FindByEmailAsync(email);
            //User user = dbContext.Users.FirstOrDefault(x => x.Id == id.ToString());
            if(user == null)
            {
                return BadRequest($"No user with email {email}");
            }

            return GetUserStat(user);
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

        private string GetUserStat(User user)
        {
            var crashes = dbContext.CrashRecords.Where(x => x.User.Id == user.Id);
            var crashCount = crashes.Count();
            return JsonConvert.SerializeObject(new { CrashCount = crashCount });
        }
    }
}
