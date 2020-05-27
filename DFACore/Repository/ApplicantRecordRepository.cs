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

        public List<City> GetCity()
        {
            var cities = new List<City>{
                new City { municipality = "Abra ", city = "Bangued" },
                new City { municipality = "Abra ", city = "Boliney" },
                new City { municipality = "Abra ", city = "Bucay" },
                new City { municipality = "Abra ", city = "Bucloc" },
                new City { municipality = "Abra ", city = "Daguioman" },
                new City { municipality = "Abra ", city = "Danglas" },
                new City { municipality = "Abra ", city = "Dolores" },
                new City { municipality = "Abra ", city = "La Paz" },
                new City { municipality = "Abra ", city = "Lacub" },
                new City { municipality = "Abra ", city = "Lagangilang" },
                new City { municipality = "Abra ", city = "Lagayan" },
                new City { municipality = "Abra ", city = "Langiden" },
                new City { municipality = "Abra ", city = "Licuan-Baay" },
                new City { municipality = "Abra ", city = "Luba" },
                new City { municipality = "Abra ", city = "Malibcong" },
                new City { municipality = "Abra ", city = "Manabo" },
                new City { municipality = "Abra ", city = "Peñarrubia" },
                new City { municipality = "Abra ", city = "Pidigan" },
                new City { municipality = "Abra ", city = "Pilar" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "Abra ", city = "" },
                new City { municipality = "", city = "" },

            };


            return cities;
        }
    }
}
