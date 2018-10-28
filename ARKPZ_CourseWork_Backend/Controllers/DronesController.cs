using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ARKPZ_CourseWork_Backend.Models;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace ARKPZ_CourseWork_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DronesController : ControllerBase
    {
        private readonly BackendContext dbContext;

        public DronesController(BackendContext context)
        {
            dbContext = context;
            //_authTokens = authTokens;
            //lock (_authTokens)
            //{
            //    // TODO TRY STANDARD AUTHORIZATION
            //    _authTokens.AddOrUpdate();
            //}
        }

        // GET: api/Drones
        [HttpGet]
        [Authorize]
        public IEnumerable<Drone> GetDrones()
        {
            return dbContext.Drones;
        }

        // GET: api/Drones/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetDrone([FromRoute] int id)
        {
            var drone = await dbContext.Drones.FindAsync(id);

            if (drone == null)
            {
                return NotFound();
            }

            return Ok(drone);
        }

        // PUT: api/Drones/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutDrone([FromRoute] int id, [FromBody] Drone drone)
        {
            dbContext.Entry(drone).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DroneExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Drones
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostDrone([FromBody] Drone drone)
        {
            dbContext.Drones.Add(drone);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction("GetDrone", new { id = drone.Id }, drone);
        }

        // DELETE: api/Drones/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDrone([FromRoute] int id)
        {
            var drone = await dbContext.Drones.FindAsync(id);
            if (drone == null)
            {
                return NotFound();
            }

            dbContext.Drones.Remove(drone);
            await dbContext.SaveChangesAsync();

            return Ok(drone);
        }

        [HttpGet("stat/{id}")]
        public ActionResult<string> GetStatistics([FromBody] int id)
        {
            Driver driver = dbContext.Drivers.FirstOrDefault(x => x.Id == id);
            if (driver == null)
            {
                return BadRequest($"No driver with Id {id}");
            }

            return GetDriverStat(driver);
        }

        private bool DroneExists(int id)
        {
            return dbContext.Drones.Any(e => e.Id == id);
        }

        private string GetDriverStat(Driver driver)
        {
            var crashes = dbContext.CrashRecords.Where(x => x.Driver.Id == driver.Id);
            var crashCount = crashes.Count();
            return JsonConvert.SerializeObject(new { CrashCount = crashCount });
        }
    }
}