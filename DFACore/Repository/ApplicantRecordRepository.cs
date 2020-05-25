using DFACore.Data;
using DFACore.Models;
using DFACore.Models.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class ApplicantRecordRepository : IApplicantRecordRepository
    {
        private ApplicationDbContext _context;
        public ApplicantRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool Add(ApplicantRecord applicantRecord)
        {
            applicantRecord.DateCreated = DateTime.UtcNow;

            _context.ApplicantRecords.Add(applicantRecord);
            _context.SaveChanges();
            return true;
        }

        public bool Delete(long id)
        {
            throw new NotImplementedException();
        }

        public bool Get(long id)
        {
            return true;
        }

        public IEnumerable<ApplicantRecord> GetAll()
        {
            throw new NotImplementedException();
        }

        public bool Update(ApplicantRecord applicantRecord)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public bool ValidateScheduleDate(DateTime date)
        {
            var count = _context.ApplicantRecords.Select(a => a.ScheduleDate).Count();
            if (count >= 50)
                return false;
            else
                return true;
        }

        public List<AvailableDAtes> GenerateListOfDates(DateTime start)
        {
            var end = start.AddDays(30);
            var dates = new List<AvailableDAtes>();
            
            for (var dt = start; dt <= end; dt = dt.AddDays(1))
            {
                if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday)
                {
                    var x = new AvailableDAtes
                    {
                        title = "Available",
                        start = dt.ToString("yyyy-MM-dd")
                    };
                    dates.Add(x);
                }
            }
            return dates;
        }

        public List<DateTime> GetUnAvailableDates()
        {

            using (var context = _context)
            {
                var commandText = "SELECT ScheduleDate, COUNT(*) as Number FROM ApplicantRecords WHERE ScheduleDate >= GETDATE() " +
                "GROUP BY ScheduleDate HAVING COUNT(*) = 1";
                context.Database.ExecuteSqlCommand(commandText);
                
            }


            var result = _context.Set<ApplicantModelDTO>();

            // use Database.ExecuteSqlCommand

            var x = result.FromSqlRaw("SELECT ScheduleDate, COUNT(*) as Number FROM ApplicantRecords WHERE ScheduleDate >= GETDATE() " +
                "GROUP BY ScheduleDate HAVING COUNT(*) = 1", "").ToList();
            return default;
        }
    }
}
