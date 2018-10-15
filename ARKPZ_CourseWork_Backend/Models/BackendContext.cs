using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARKPZ_CourseWork_Backend.Models;

namespace ARKPZ_CourseWork_Backend.Models
{
    public class BackendContext: DbContext
    {
        public virtual DbSet<Drone> Drones { get; set; }
        public virtual DbSet<CrashRecord> CrashReports { get; set; }
        public virtual DbSet<Driver> Drivers { get; set; }

        public BackendContext(DbContextOptions<BackendContext> dbContextOptions) :
            base(dbContextOptions)
        {
            Database.EnsureCreated();
        }

        public DbSet<ARKPZ_CourseWork_Backend.Models.Coordinates> Coordinates { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<CrashReport>().OwnsOne(parent => parent.Coords);
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
