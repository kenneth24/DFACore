using DFACore.Data;
using DFACore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class AdministrationRepository
    {
        private ApplicationDbContext _context;
        public AdministrationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<ApplicantRecord> GetAllApplicantRecord(string sortOrder, string searchString)
        {
            //var result = _context.ApplicantRecords.Select(a => a);
            var applicants = from s in _context.ApplicantRecords
                             select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                applicants = applicants.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString)
                                       || s.ApplicationCode.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    applicants = applicants.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    applicants = applicants.OrderBy(s => s.ScheduleDate);
                    break;
                case "date_desc":
                    applicants = applicants.OrderByDescending(s => s.ScheduleDate);
                    break;
                default:
                    applicants = applicants.OrderBy(s => s.LastName);
                    break;
            }
            var result = applicants.AsNoTracking();

            return result;
        }

    }
}
