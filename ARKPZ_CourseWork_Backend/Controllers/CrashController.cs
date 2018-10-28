using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARKPZ_CourseWork_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebSocketSharp;
//using System.Web.Script.Serialization;

namespace ARKPZ_CourseWork_Backend.Controllers
{
    [Route("accr/[controller]")]
    [ApiController]
    public class CrashController : ControllerBase
    {
        // POST api/values
        private Dictionary<int, string> DroneAddresses = new Dictionary<int, string>
        {

        };
        private readonly BackendContext dbContext;
        private DateTime arrivalTime;
        public CrashController(BackendContext context)
        {
            dbContext = context;
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

        [HttpPost]
        [Authorize]
        public IActionResult Crash([FromBody] CrashReport crashReport)
        {
            int driverId = crashReport.DriverId;
            var driver = dbContext.Drivers.FirstOrDefault(x => x.Id == driverId);
            if (driver is null)
            {
                return Unauthorized();
            }
            if (driver.TrustLevel < 5)
            {
                return Ok("untrustworthy");
            }
            var crashRecord = new CrashRecord()
            {
                Driver = driver,
                Coords = crashReport.Coords,
            };
            Drone nearestDrone = GetNearestDrone(crashRecord.Coords);
            if (nearestDrone is null)
            {
                return Ok("no-drone");
                // handle this case
                // return "No drones available, but we will call ambulance"
            }
            crashRecord.AssignedDrone = nearestDrone;

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
            return Ok(arrival.Minutes);
            //return new JsonResult(new object()) { StatusCode = 200 };
        }

        private Drone GetNearestDrone(Coordinates coords)
        {
            var drones = dbContext.Drones;
            var nearestDrone = drones.Where(x => x.Status == "ok")
                .DefaultIfEmpty()
                .Min();

            return nearestDrone;
        }

        //[Authorize]
        [HttpGet("test")]
        public string Test()
        {
            var drones = dbContext.Drones;
            return string.Join("\n", drones);
        }

        [HttpGet("stat/{id}")]
        public ActionResult<string> GetStatistics([FromBody] int id)
        {
            Driver driver = dbContext.Drivers.FirstOrDefault(x => x.Id == id);
            if(driver == null)
            {
                return BadRequest($"No driver with Id {id}");
            }

            return GetDriverStat(driver);
        }

        private TimeSpan GetArrivalTimeTest(int droneId, Coordinates coords)
        {
            string droneAddress = DroneAddresses[droneId];
            var socket = new WebSocket(droneAddress);
            socket.OnMessage += OnDroneMessageReceivedTest;
            string requestFormatted = FormatArrivalTimeRequestTest(coords);
            //socket.Send(requestFormatted);

            return DateTime.Now - arrivalTime;
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

        private void OnDroneMessageReceivedTest(object sender, MessageEventArgs e)
        {
            var data = JsonConvert.DeserializeObject<DateTime>(e.Data);
            arrivalTime = data;
        }

        private string GetDriverStat(Driver driver)
        {
            var crashes = dbContext.CrashRecords.Where(x => x.Driver.Id == driver.Id);
            var crashCount = crashes.Count();
            return JsonConvert.SerializeObject(new { CrashCount = crashCount });
        }
    }
}
