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
        public DbSet<Models.Holiday> Holidays { get; set; }

        public DbSet<Models.Branch> Branches { get; set; }
        public DbSet<Models.Price> Prices { get; set; }
        public DbSet<Models.Notice> Notices { get; set; }
        public DbSet<Models.tbl_attendance> tbl_attendance { get; set; } // see comments on class

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Models.ApplicantRecord>()
            .HasIndex(u => u.ApplicationCode)
            .IsUnique();

            builder.Entity<UnavailableDate>(e =>
            {
                e.HasNoKey();
                e.ToView(null);
            });

            //NOTE: I manually add index in ApplicantRecords table
            //create index IX_ApplicantRecords_Branchid_Scheduledate
            //on ApplicantRecords(branchid, scheduledate);

            //builder.Entity<ApplicantRecord>()
            //    .HasIndex(u => new { u.BranchId, u.ScheduleDate })
            //    .IsClustered(false);

            builder.Entity<AdminAccountViewModel>(e =>
            {
                e.HasNoKey();
                e.ToView(null);
            });

            builder.Entity<UserAccountViewModel>(e =>
            {
                e.HasNoKey();
                e.ToView(null);
            });
        }
    }
}
