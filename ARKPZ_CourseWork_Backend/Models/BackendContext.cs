using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARKPZ_CourseWork_Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ARKPZ_CourseWork_Backend.Models
{
    public class BackendContext: IdentityDbContext<User>
    {
        public virtual DbSet<Drone> Drones { get; set; }
        public virtual DbSet<CrashRecord> CrashRecords { get; set; }
        //public virtual DbSet<User> Users { get; set; }

        public BackendContext(DbContextOptions<BackendContext> dbContextOptions) :
            base(dbContextOptions)
        {
            Database.EnsureCreated();
            //Database.Migrate();
        }

        public DbSet<Coordinates> Coordinates { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<CrashReport>().OwnsOne(parent => parent.Coords);
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
