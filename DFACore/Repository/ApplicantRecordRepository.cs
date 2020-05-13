using DFACore.Data;
using DFACore.Models;
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
            _context.ApplicantRecords.Add(applicantRecord);
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
    }
}
