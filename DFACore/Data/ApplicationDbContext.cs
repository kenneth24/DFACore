using System;
using System.Collections.Generic;
using System.Text;
using DFACore.Models;
using DFACore.Repository;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DFACore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.ApplicantRecord> ApplicantRecords { get; set; }
        public DbSet<Models.ActivityLog> ActivityLogs { get; set; }
        public DbSet<Models.DisabledDate> DisabledDates { get; set; }

        public DbSet<Models.ScheduleCapacity> ScheduleCapacities { get; set; }
    }
}
